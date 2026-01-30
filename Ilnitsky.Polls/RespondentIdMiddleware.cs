using Ilnitsky.Polls.BusinessLogic;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Ilnitsky.Polls;

public class RespondentIdMiddleware(RequestDelegate _next)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        string respondentId;

        if (httpContext.Request.Cookies.TryGetValue("RespondentId", out var id))
        {
            respondentId = id;
        }
        else
        {
            respondentId = GuidHelper.CreateGuidV7().ToString();
        }

        httpContext.Response.Cookies.Append(
            "RespondentId",
            respondentId,
            new CookieOptions
            {
                MaxAge = TimeSpan.FromDays(400),
                HttpOnly = true,
                Secure = true,
                IsEssential = true,
            });

        await _next.Invoke(httpContext);
    }
}
