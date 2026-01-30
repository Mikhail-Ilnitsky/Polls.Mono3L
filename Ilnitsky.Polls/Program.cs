using Ilnitsky.Polls;
using Ilnitsky.Polls.BusinessLogic;
using Ilnitsky.Polls.BusinessLogic.Handlers.Polls;
using Ilnitsky.Polls.DataAccess;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
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

builder.Services.AddTransient<GetPollsHandler>();

var app = builder.Build();

app.Use(async (httpContext, next) =>
{
    if (httpContext.Request.Cookies.TryGetValue("RespondentId", out var respondentId))
    {
        httpContext.Response.Cookies.Append(
            "RespondentId",
            respondentId,
            new Microsoft.AspNetCore.Http.CookieOptions
            {
                MaxAge = TimeSpan.FromDays(400),
                HttpOnly = true,
                Secure = true,
                IsEssential = true,
            });
    }
    else
    {
        httpContext.Response.Cookies.Append(
            "RespondentId",
            GuidHelper.CreateGuidV7().ToString(),
            new Microsoft.AspNetCore.Http.CookieOptions
            {
                MaxAge = TimeSpan.FromDays(400),
                HttpOnly = true,
                Secure = true,
                IsEssential = true,
            });
    }

    await next.Invoke();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseDefaultFiles();  // добавляем поддержку страниц html по умолчанию
app.UseStaticFiles();   // добавляем поддержку статических файлов

app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

await app.MigrateAsync<ApplicationDbContext>();
await app.InitAsync<DbInitializer>();

app.Run();
