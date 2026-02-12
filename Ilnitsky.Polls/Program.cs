using Ilnitsky.Polls.BusinessLogic.Handlers.Answers;
using Ilnitsky.Polls.BusinessLogic.Handlers.Polls;
using Ilnitsky.Polls.DataAccess;
using Ilnitsky.Polls.DbInitialization;
using Ilnitsky.Polls.Filters;
using Ilnitsky.Polls.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

builder.Services.AddControllers(
    options => options.Filters.Add<ErrorResultFilter>());       // Добавляем фильтр для сохранения информации об ошибках

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var dbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(dbConnectionString))
{
    throw new InvalidOperationException("Connection string for ApplicationDbContext is not configured.");
}

builder.Services.AddDbContext<ApplicationDbContext>(
    optionsBuilder => optionsBuilder
        .UseLazyLoadingProxies()
        .UseMySql(dbConnectionString, ServerVersion.AutoDetect(dbConnectionString)));
builder.Services.AddTransient<DbInitializer>();

// Хэндлеры
builder.Services.AddTransient<GetPollLinksHandler>();
builder.Services.AddTransient<GetPollByIdHandler>();
builder.Services.AddTransient<CreateRespondentAnswerHandler>();

builder.Services.AddDistributedMemoryCache();       // Добавляем IDistributedMemoryCache для хранения данных сессий
builder.Services.AddSession();                      // Добавляем сервисы сессии

var app = builder.Build();

app.UseSession();                                   // Добавляем middleware для работы с сессиями

app.UseMiddleware<ErrorLoggingMiddleware>();        // Логируем явные ошибки и обрабатываем (и логируем) необработанные исключения
app.UseMiddleware<RespondentMiddleware>();          // Проверяем наличие у респондента respondentId, и создаём его, либо обновляем
app.UseMiddleware<RespondentSessionMiddleware>();   // Проверяем наличие у респондента respondentSessionId, и создаём его, если его нет

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseDefaultFiles();                              // Добавляем поддержку страниц html по умолчанию
app.UseStaticFiles();                               // Добавляем поддержку статических файлов

app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

await app.MigrateAsync<ApplicationDbContext>();
await app.InitAsync<DbInitializer>();

app.Run();
