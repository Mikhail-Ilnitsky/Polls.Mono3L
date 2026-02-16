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

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())    // Простая проверка
    .AddSqlServer(                                          // Проверка доступности БД
        connectionString: dbConnectionString,
        name: "sql_server",
        failureStatus: HealthStatus.Degraded,
        tags: ["ready"])
    .ForwardToPrometheus();

builder.Services.AddControllers(
    options => options.Filters.Add<ErrorResultFilter>());   // Добавляем фильтр для сохранения информации об ошибках

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(
    optionsBuilder => optionsBuilder
        .UseLazyLoadingProxies()
        .UseMySql(dbConnectionString, ServerVersion.AutoDetect(dbConnectionString)));
builder.Services.AddTransient<DbInitializer>();

// Добавляем хэндлеры
builder.Services.AddTransient<GetPollLinksHandler>();
builder.Services.AddTransient<GetPollByIdHandler>();
builder.Services.AddTransient<CreateRespondentAnswerHandler>();

builder.Services.AddDistributedMemoryCache();       // Добавляем IDistributedMemoryCache для хранения данных сессий
builder.Services.AddSession();                      // Добавляем сервисы сессии

//=================================================================================================================//

var app = builder.Build();

app.UseHttpsRedirection();                          // Перенаправляем HTTP-запросы на HTTPS

app.UseSession();                                   // Добавляем middleware для работы с сессиями

app.UseMiddleware<ErrorLoggingMiddleware>();        // Логируем явные ошибки и обрабатываем (и логируем) необработанные исключения
app.UseMiddleware<RespondentMiddleware>();          // Проверяем наличие у респондента respondentId, и создаём его, либо обновляем
app.UseMiddleware<RespondentSessionMiddleware>();   // Проверяем наличие у респондента respondentSessionId, и создаём его, если его нет

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();                              // Добавляем поддержку страниц html по умолчанию
app.UseStaticFiles();                               // Добавляем поддержку статических файлов

app.UseRouting();                                   // Определяем конечные маршруты

app.UseWhen(                                        // Не собираем метрики если путь начинается с /metrics или /health
    context =>
        !context.Request.Path.StartsWithSegments("/metrics", StringComparison.InvariantCultureIgnoreCase)
        && !context.Request.Path.StartsWithSegments("/health", StringComparison.InvariantCultureIgnoreCase),
    appBuilder => appBuilder.UseHttpMetrics());     // Собираем метрики с информацией о маршруте

//app.UseAuthorization();

app.MapControllers();                               // Региструем контроллеры API

app.MapHealthChecks("/health");                                     // Базовая проверка состояния для Kubernetes/DockerSwarm
app.MapHealthChecks("/health/live", new HealthCheckOptions          // Только проверка "self"
{
    Predicate = _ => false
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions         // Проверка состояния включающая доступность БД
{
    Predicate = (check) => check.Tags.Contains("ready")
});

app.MapMetrics();                                   // Отдаём метрики по адресу /metrics (по умолчанию)
app.MapFallbackToFile("index.html");                // Перенаправляем на index.html

await app.MigrateAsync<ApplicationDbContext>();
await app.InitAsync<DbInitializer>();

app.Run();
