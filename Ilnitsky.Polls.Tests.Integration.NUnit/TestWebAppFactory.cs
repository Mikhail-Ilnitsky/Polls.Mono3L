using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Ilnitsky.Polls.Tests.Integration.NUnit;

public class TestWebAppFactory(string dbConnectionString, string redisConnectionString) : WebApplicationFactory<Program>
{
    private readonly string _dbConnectionString = dbConnectionString ?? throw new ArgumentNullException(nameof(dbConnectionString));
    private readonly string _redisConnectionString = redisConnectionString ?? throw new ArgumentNullException(nameof(redisConnectionString));

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        // Этот метод вызывается в самом начале построения Host, 
        // перекрывая параметры до вызова AutoDetect в Program.cs
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _dbConnectionString,
                ["ConnectionStrings:Redis"] = _redisConnectionString
            });
        });

        //return base.CreateHost(builder);

        // Отключаем валидацию DI для тестов, чтобы проверить, в ней ли дело
        builder.UseDefaultServiceProvider((context, options) =>
        {
            options.ValidateScopes = false;
            options.ValidateOnBuild = false;
        });

        try
        {
            return base.CreateHost(builder);
        }
        catch (Exception ex)
        {
            // Если приложение падает при старте хоста, мы увидим это в логах NUnit
            Console.WriteLine($"!!! ФАТАЛЬНЫЙ СБОЙ ИНИЦИАЛИЗАЦИИ ХОСТА: {ex}");
            throw;
        }
    }
}
