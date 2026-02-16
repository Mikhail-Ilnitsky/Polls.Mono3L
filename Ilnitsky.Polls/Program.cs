using Ilnitsky.Polls;
using Ilnitsky.Polls.BusinessLogic.Handlers.Answers;
using Ilnitsky.Polls.BusinessLogic.Handlers.Polls;
using Ilnitsky.Polls.DataAccess;
using Ilnitsky.Polls.DbInitialization;
using Ilnitsky.Polls.Filters;
using Ilnitsky.Polls.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Prometheus;
using Serilog;
using System;

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

var dbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(dbConnectionString))
{
    throw new InvalidOperationException("Connection string for ApplicationDbContext is not configured.");
}

builder.Services.AddHealthChecks()                          // Регистрируем сервисы мониторинга состояния
    .AddCheck("self", () => HealthCheckResult.Healthy())    // Простая проверка
    .AddSqlServer(                                          // Проверка доступности БД
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
builder.Services.AddSwaggerGen();                           // Регистрируем генератор документации API

builder.Services.AddDistributedMemoryCache();               // Регистрируем IDistributedMemoryCache для хранения данных сессий
builder.Services.AddSession();                              // Регистрируем сервисы сессии

// Регистрируем хэндлеры
builder.Services.AddTransient<GetPollLinksHandler>();
builder.Services.AddTransient<GetPollByIdHandler>();
builder.Services.AddTransient<CreateRespondentAnswerHandler>();

//=================================================================================================================//

var app = builder.Build();

app.UseHttpsRedirection();                          // Подключаем перенаправление HTTP-запросов на HTTPS

app.UseSession();                                   // Подключаем использование сессий

app.UseMiddleware<ErrorLoggingMiddleware>();        // Логируем явные ошибки и обрабатываем (и логируем) необработанные исключения
app.UseMiddleware<RespondentMiddleware>();          // Проверяем наличие у респондента respondentId, и создаём его, либо обновляем
app.UseMiddleware<RespondentSessionMiddleware>();   // Проверяем наличие у респондента respondentSessionId, и создаём его, если его нет

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();                               // Подключаем создание документации API
    app.UseSwaggerUI();                             // Подключаем отображение страницы Swagger
}

app.UseDefaultFiles();                              // Подключаем поддержку страниц html по умолчанию
app.UseStaticFiles();                               // Подключаем поддержку статических файлов

app.UseRouting();                                   // Подключаем определение конечных точек

app.UseWhen(                                        // Не собираем метрики если путь начинается с /metrics или /health
    context =>
        !context.Request.Path.StartsWithSegments("/metrics", StringComparison.InvariantCultureIgnoreCase)
        && !context.Request.Path.StartsWithSegments("/health", StringComparison.InvariantCultureIgnoreCase),
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
