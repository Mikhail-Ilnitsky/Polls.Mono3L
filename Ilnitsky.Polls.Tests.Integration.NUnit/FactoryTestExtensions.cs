using System.Net;
using System.Net.Http;

using Microsoft.AspNetCore.Mvc.Testing;

using Microsoft.AspNetCore.Mvc.Testing.Handlers;

namespace Ilnitsky.Polls.Tests.Integration.NUnit;

public static class FactoryTestExtensions
{
    public static HttpClient GetHttpClientWithCookies(this WebApplicationFactory<Program> factory)
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            // Автоматически сохранять куки между запросами одного клиента
            HandleCookies = true,
            // (Опционально) Запретить автоматические редиректы, чтобы проверять 301/302 ответы
            AllowAutoRedirect = false,
        });
    }

    public static HttpClient GetHttpClientWithCookieContainer(
        this WebApplicationFactory<Program> factory,
        CookieContainer cookieContainer)
    {
        return factory.CreateDefaultClient(new CookieContainerHandler(cookieContainer));
    }
}
