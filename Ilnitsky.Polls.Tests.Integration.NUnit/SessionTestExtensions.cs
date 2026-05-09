using System.Net;
using System.Text;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Ilnitsky.Polls.Tests.Integration.NUnit;

public static class SessionTestExtensions
{
    public static string? GetValueFromSession(
        this WebApplicationFactory<Program> factory,
        Cookie sessionCookie,
        string key)
    {
        // Получаем системные службы ASP.NET Core
        var services = factory.Services;
        var dataProtection = services.GetRequiredService<IDataProtectionProvider>();
        var sessionStore = services.GetRequiredService<ISessionStore>();

        // Настраиваем (Data Protection), используя ту же цель, что и сервер сессий
        // Важно: в ASP.NET Core строка цели жестко зашита как "SessionMiddleware"
        var protector = dataProtection.CreateProtector("SessionMiddleware");

        // Декодируем URL-символы (такие как %2B, %3D) обратно в сырой Base64Url
        var unescapedCookieValue = WebUtility.UrlDecode(sessionCookie.Value);

        // Расшифровываем значение куки в сырой SessionKey (ключ в памяти)
        var protectedBytes = WebEncoders.Base64UrlDecode(unescapedCookieValue);
        var plainBytes = protector.Unprotect(protectedBytes);

        // Первые 16 байт в куке сессии ASP.NET Core — это внутренний ключ сессии
        var sessionKeyBytes = new byte[16];
        Array.Copy(plainBytes, sessionKeyBytes, 16);
        string internalSessionKey = new Guid(sessionKeyBytes).ToString();

        // Загружаем сессию из InMemory хранилища напрямую по ключу
        // Передаем пустые заглушки для параметров, так как нам нужно только чтение
        var session = sessionStore.Create(
            internalSessionKey,
            TimeSpan.FromMinutes(20),
            TimeSpan.FromMinutes(20),
            () => true,
            isNewSessionKey: false);

        session.LoadAsync().GetAwaiter().GetResult();

        // Читаем строку данных по ключу данных сохранённых в сессии
        return session.GetString(key);
    }
}
