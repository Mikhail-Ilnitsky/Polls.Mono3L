using System;
using System.IO;

using Ilnitsky.Polls.BusinessLogic.Handlers.Answers;
using Ilnitsky.Polls.BusinessLogic.Handlers.Polls;
using Ilnitsky.Polls.DataAccess;
using Ilnitsky.Polls.DbInitialization;
using Ilnitsky.Polls.Enrichers;
using Ilnitsky.Polls.Filters;
using Ilnitsky.Polls.Middlewares;
using Ilnitsky.Polls.Services.OptionsProviders;
using Ilnitsky.Polls.Services.Redis;
using Ilnitsky.Polls.Services.Settings;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

using Polly;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Retry;

using Prometheus;

using Serilog;

using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Подключаем Serilog как основной провайдер логов для хоста
builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.With<CustomUtcDateTimeEnricher>()
    .Enrich.With<CustomUtcTimestampEnricher>()
    .Enrich.FromLogContext()
    .Enrich.WithThreadId()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName());

// Настраиваем проксирование заголовков
var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
// Вместо ручной очистки списков используем ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
//forwardedOptions.KnownNetworks.Clear();                     // Удаляем список доверенных подсетей (по умолчанию там loopback/localhost, т. е. 127.0.0.1)
//forwardedOptions.KnownProxies.Clear();                      // Удаляем список конкретных IP-адресов доверенных прокси-серверов

if (builder.Environment.IsProduction())
{
    var keysPath = "/app/dp-keys";                          // Путь внутри контейнера, где будем хранить ключи шифрования

    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
        .SetApplicationName("pool-app");                    // Присваиваем приложению имя, чтобы задать "соль" для шифрования
}

var dbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(dbConnectionString))
{
    throw new InvalidOperationException("Connection string for ApplicationDbContext is not configured.");
}
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
if (string.IsNullOrWhiteSpace(redisConnectionString))
{
    throw new InvalidOperationException("Connection string for Redis is not configured.");
}

builder.Services.AddHealthChecks()                          // Регистрируем сервисы мониторинга состояния
    .AddCheck("self", () => HealthCheckResult.Healthy())    // Простая проверка
    .AddMySql(                                              // Проверка доступности БД
        connectionString: dbConnectionString,
        name: "sql_server",
        failureStatus: HealthStatus.Degraded,
        tags: ["ready"])
    .ForwardToPrometheus();                                 // Добавляем в метрики aspnetcore_healthcheck_status

builder.Services.AddDbContext<ApplicationDbContext>(        // Регистрируем контекст базы данных и настраиваем подключение
    optionsBuilder => optionsBuilder
        .UseLazyLoadingProxies()
        .UseMySql(dbConnectionString, ServerVersion.AutoDetect(dbConnectionString)));

builder.Services.AddTransient<DbInitializer>();             // Регистрируем инициализатор базы данных

builder.Services.AddControllers(                            // Регистрируем сервисы контроллеров
    options => options.Filters.Add<ErrorResultFilter>());   // Регистрируем фильтр для сохранения информации об ошибках

// Learn more at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();                 // Регистрируем обнаружитель конечных точек для minimalAPI
builder.Services.AddSwaggerGen(options =>                   // Регистрируем генератор документации API
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Ilnitsky.Polls.Mono3L API Опросов", Version = "1.0" });
    var xmlPath = Path.Combine(AppContext.BaseDirectory, "Api.xml");
    options.IncludeXmlComments(xmlPath);                    // Подключаем XML-комментарии из проекта
});

builder.Services.AddDistributedMemoryCache();               // Регистрируем IDistributedMemoryCache для хранения данных сессий в памяти процесса
// Регистрируем кэширование в Redis в этом случае и сессии тоже хранятся в более медленном Redis!
// Поэтому прямое использование Redis замедляет доступ к сессиям! 
//builder.Services.AddStackExchangeRedisCache(options =>    // Регистрируем IDistributedMemoryCache для хранения кэша в Redis, но в этом случае и сессии тоже хранятся в более медленном Redis!
//{
//    options.Configuration = redisConnectionString;        // Строка подключения Redis
//    options.InstanceName = "PollsApp_";                   // Префикс для ключей
//});

builder.Services.AddSession();                              // Регистрируем сервисы сессии

// Для своих задач регистрируем Redis напрямую (не как DistributedCache, чтобы Redis не брал на себя кэширование сессий)
var multiplexer = ConnectionMultiplexer.Connect(redisConnectionString);                 // Создаём менеджер подключений к Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);                     // Регистрируем менеджер подключений Redis в DI
builder.Services.AddScoped<IRedisService, RedisService>();                              // Регистрируем сервис для взаимодействия с Redis

builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("Cache"));   // Регистрируем секцию настроек кэширования

builder.Services.AddSingleton<ICacheOptionsProvider, CacheOptionsProvider>();           // Регистрируем провайдер настроек кэширования


// Создаем общий "предохранитель" для пайплайнов Redis, чтобы проблемы в одном методе отключали и остальные
var redisCircuitOptions = new CircuitBreakerStrategyOptions
{
    FailureRatio = 0.5,                             // "Размыкаем цепь", если 50% запросов упали
    SamplingDuration = TimeSpan.FromSeconds(30),    // Окно статистики, анализируем последние 30 секунд
    MinimumThroughput = 5,                          // Минимум 5 запросов для "размыкания цепи"
    BreakDuration = TimeSpan.FromSeconds(15)        // Сколько врменеи не трогать Redis после "размыкания цепи"
};

// Настраиваем пайплайн Redis для ЧТЕНИЯ (максимум попыток доступа)
builder.Services.AddResiliencePipeline<string, object>("redis-get", (piplineBuilder, context) =>
{
    var logger = context.ServiceProvider.GetRequiredService<ILogger<RedisService>>();

    // Подписываем общий объект на логи (один раз здесь достаточно)
    redisCircuitOptions.OnOpened = args =>                  // Обрабатываем случай "разрыва цепи"
    {
        logger.LogCritical("Redis DOWN: Circuit opened for {Duration}s", args.BreakDuration.TotalSeconds);
        return default;
    };
    redisCircuitOptions.OnClosed = _ =>                     // Обрабатываем случай "замыкания цепи"
    {
        logger.LogInformation("Redis UP: Circuit closed, cache resumed");
        return default;
    };

    piplineBuilder
        .AddFallback(new FallbackStrategyOptions<object>
        {
            FallbackAction = _ => Outcome.FromResultAsValueTask<object>(new RedisServiceResult(false)),
            OnFallback = args =>
            {
                logger.LogWarning(args.Outcome.Exception, "Redis GET failed (Timeout/Error)");
                return default;
            }
        })
        .AddTimeout(TimeSpan.FromMilliseconds(600))         // Общий таймаут на все попытки + паузы вместе
        .AddCircuitBreaker(redisCircuitOptions)             // Прекращаем попытки на заданное время, если Redis недоступен
        .AddRetry(new RetryStrategyOptions<object>          // Выполняем повторные попытки запросов к Redis
        {
            ShouldHandle = new PredicateBuilder<object>()
                .Handle<Exception>()                       // Обрабатывать все ошибки (все типы исключений)
                .HandleResult(result => result is RedisServiceResult { IsAvailable: false }), // Обрабатывать наш вариант результата (если он вернулся вместо исключения)
            MaxRetryAttempts = 3,                           // Количество повторов (итого 4 попытки: 1 основная + 3 дополнительных)
            BackoffType = DelayBackoffType.Exponential,     // Экспоненциальная задержка: каждая следующая попытка будет через увеличивающийся интервал (например, 20мс, 40мс, 80мс)
            UseJitter = true,                               // Джиттер добавляет случайное смещение к задержке (+/- несколько мс), чтобы много запросов не ударили по Redis одновременно в одну и ту же секунду после паузы
            Delay = TimeSpan.FromMilliseconds(20),          // Базовое время ожидания перед первой повторной попыткой
            OnRetry = args =>
            {
                logger.LogWarning(
                    "Redis GET Retry {Attempt}. Error: {Error}",
                    args.AttemptNumber + 1,
                    args.Outcome.Exception?.Message);
                return default;
            }
        })
        .AddTimeout(TimeSpan.FromMilliseconds(100));        // Таймаут текущей попытки, ждем ответа от Redis не более 100мс за раз
});

// Настраиваем пайплайн Redis для ЗАПИСИ (минимум задержек)
builder.Services.AddResiliencePipeline<string, object>("redis-set", (piplineBuilder, context) =>
{
    var logger = context.ServiceProvider.GetRequiredService<ILogger<RedisService>>();

    piplineBuilder
        .AddFallback(new FallbackStrategyOptions<object>
        {
            FallbackAction = _ => Outcome.FromResultAsValueTask<object>(new RedisServiceResult(false)),
            OnFallback = args =>
            {
                logger.LogWarning(args.Outcome.Exception, "Redis SET failed (Timeout/Error)");
                return default;
            }
        })
        .AddCircuitBreaker(redisCircuitOptions)             // Прекращаем попытки на заданное время, если Redis недоступен
        .AddTimeout(TimeSpan.FromMilliseconds(100));        // Если сетевой вызов к Redis длится дольше 100мс, принудительно обрываем соединение
});

// Настраиваем пайплайн Redis для УДАЛЕНИЯ (средний приоритет)
builder.Services.AddResiliencePipeline<string, object>("redis-remove", (piplineBuilder, context) =>
{
    var logger = context.ServiceProvider.GetRequiredService<ILogger<RedisService>>();

    piplineBuilder
        .AddFallback(new FallbackStrategyOptions<object>
        {
            FallbackAction = _ => Outcome.FromResultAsValueTask<object>(new RedisServiceResult(false)),
            OnFallback = args =>
            {
                logger.LogWarning(args.Outcome.Exception, "Redis REMOVE failed (Timeout/Error)");
                return default;
            }
        })
        .AddCircuitBreaker(redisCircuitOptions)             // Прекращаем попытки на заданное время, если Redis недоступен
        .AddRetry(new RetryStrategyOptions                  // Выполняем повторные попытки запросов к Redis
        {
            MaxRetryAttempts = 1,                           // Количество повторов (итого 2 попытки: 1 основная + 1 дополнительная)
            Delay = TimeSpan.FromMilliseconds(50),          // Базовое время ожидания перед первой повторной попыткой
            OnRetry = args =>
            {
                logger.LogWarning(
                    "Redis REMOVE Retry {Attempt}. Error: {Error}",
                    args.AttemptNumber + 1,
                    args.Outcome.Exception?.Message);
                return default;
            }
        })
        .AddTimeout(TimeSpan.FromMilliseconds(150));        // Таймаут текущей попытки, ждем ответа от Redis не более 150мс за раз
});

// Регистрируем хэндлеры
builder.Services.AddTransient<GetPollLinksHandler>();
builder.Services.AddTransient<GetPollByIdHandler>();
builder.Services.AddTransient<CreateRespondentAnswerHandler>();

//=================================================================================================================//

var app = builder.Build();

app.UseMiddleware<ErrorLoggingMiddleware>();        // Логируем явные ошибки и обрабатываем (и логируем) необработанные исключения

app.UseForwardedHeaders(forwardedOptions);          // Подключаем распозначание протокола в случае наличия обратного прокси
//app.UseHsts();                                    // ТОЛЬКО НА СЕРВЕРЕ! Запрещаем браузеру впредь обращаться не по HTTPS

app.UseWhen(                                        // Не перенаправляем если путь начинается с /metrics или /health
    context =>
        !context.Request.Path.StartsWithSegments("/metrics", StringComparison.OrdinalIgnoreCase)
        && !context.Request.Path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase),
    appBuilder =>
    {
        appBuilder.UseHttpsRedirection();           // Подключаем перенаправление HTTP-запросов на HTTPS
    });

app.UseDefaultFiles();                              // Подключаем поддержку страниц html по умолчанию
app.UseStaticFiles();                               // Подключаем поддержку статических файлов

app.UseWhen(                                        // Проверяем respondentId только для API (кроме кэшируемого метода получения списка опросов)
    context =>
        context.Request.Path.StartsWithSegments("/api/v1")                                                              // Только API
        && !(context.Request.Path.Value?.TrimEnd('/') == "/api/v1/polls" && HttpMethods.IsGet(context.Request.Method)), // Только не метод получения списка опросов
    appBuilder =>
    {
        appBuilder.UseSession();                                   // Подключаем использование сессий
        appBuilder.UseMiddleware<RespondentMiddleware>();          // Проверяем наличие у респондента respondentId, и создаём его, либо обновляем
        appBuilder.UseMiddleware<RespondentSessionMiddleware>();   // Проверяем наличие у респондента respondentSessionId, и создаём его, если его нет
    });

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();                               // Подключаем создание документации API
    app.UseSwaggerUI();                             // Подключаем отображение страницы Swagger
}

app.UseRouting();                                   // Подключаем определение конечных точек

app.UseWhen(                                        // Не собираем метрики если путь начинается с /metrics или /health
    context =>
        !context.Request.Path.StartsWithSegments("/metrics", StringComparison.OrdinalIgnoreCase)
        && !context.Request.Path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase),
    appBuilder => appBuilder.UseHttpMetrics());     // Собираем метрики с информацией о маршруте

//app.UseAuthorization();

app.MapControllers();                               // Подключаем контроллеры API

app.MapHealthChecks("/health");                     // Базовая проверка состояния для Kubernetes/DockerSwarm
app.MapHealthChecks("/health/live",                 // Только проверка "self"
    new HealthCheckOptions
    {
        Predicate = _ => false
    });
app.MapHealthChecks("/health/ready",                // Проверка состояния включающая доступность БД
    new HealthCheckOptions
    {
        Predicate = (check) => check.Tags.Contains("ready")
    });

app.MapMetrics();                                   // Отдаём метрики по адресу /metrics (по умолчанию)
app.MapFallbackToFile("index.html");                // Перенаправляем на index.html

await app.MigrateAsync<ApplicationDbContext>();     // Выполняем миграцию БД
await app.InitAsync<DbInitializer>();               // Выполняем инициализацию БД

app.Run();
