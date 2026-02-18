using Ilnitsky.Polls.DbInitialization;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System.Threading.Tasks;

namespace Ilnitsky.Polls.DbInitialization;

public static class DbInitializationExtensions
{
    public static async Task InitAsync<T>(this IHost host)
        where T : IDbInitializer
    {
        using IServiceScope scope = host.Services.CreateScope();
        using T dbInitializer = scope.ServiceProvider.GetRequiredService<T>();
        await dbInitializer.InitDatabaseAsync();
    }

    public static async Task MigrateAsync<T>(this IHost host)
        where T : DbContext
    {
        using IServiceScope scope = host.Services.CreateScope();
        using T dbContext = scope.ServiceProvider.GetRequiredService<T>();
        await dbContext.Database.MigrateAsync();
    }
}
