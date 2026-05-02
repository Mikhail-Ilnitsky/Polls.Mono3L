using System;
using System.Collections.Generic;

using Ilnitsky.Polls.DataAccess;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Moq;

namespace Ilnitsky.Polls.Tests.XUnit.Middlewares;

public static class ContextHelper
{
    public static DefaultHttpContext CreateHttpContext(ApplicationDbContext dbContext)
    {
        var httpContext = new DefaultHttpContext();

        // Настраиваем ServiceProvider для получения DbContext через RequestServices
        var serviceProvider = new ServiceCollection()
            .AddSingleton(dbContext)
            .BuildServiceProvider();
        httpContext.RequestServices = serviceProvider;

        // Мокаем сессию
        var sessionMock = new Mock<ISession>();
        var sessionData = new Dictionary<string, byte[]>();

        sessionMock
            .Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Callback<string, byte[]>((key, val) => sessionData[key] = val);
        sessionMock
            .Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]?>.IsAny))
            .Returns((string key, out byte[]? val) => sessionData.TryGetValue(key, out val));
        sessionMock
            .Setup(s => s.Keys)
            .Returns(sessionData.Keys);

        httpContext.Session = sessionMock.Object;
        return httpContext;
    }

    public static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }
}
