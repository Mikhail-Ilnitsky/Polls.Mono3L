using Ilnitsky.Polls.BusinessLogic;
using Ilnitsky.Polls.DataAccess;
using Ilnitsky.Polls.DataAccess.Entities.Answers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ilnitsky.Polls;

public class RespondentMiddleware(RequestDelegate _next)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        Guid respondentId;
        string respondentIdString;

        if (httpContext.Request.Cookies.TryGetValue("RespondentId", out var id))
        {
            respondentIdString = id;

            if (!httpContext.Session.Keys.Contains("RespondentSessionId"))
            {
                SaveInSession(httpContext, respondentIdString);
            }
        }
        else
        {
            respondentId = GuidHelper.CreateGuidV7();
            respondentIdString = respondentId.ToString();

            await SaveInDatabaseAsync(httpContext, respondentId);
            SaveInSession(httpContext, respondentIdString);
        }

        SaveInCookies(httpContext, respondentIdString);

        await _next.Invoke(httpContext);
    }

    private static void SaveInCookies(HttpContext httpContext, string respondentIdString)
    {
        httpContext.Response.Cookies.Append(
            "RespondentId",
            respondentIdString,
            new CookieOptions
            {
                MaxAge = TimeSpan.FromDays(400),
                HttpOnly = true,
                Secure = true,
                IsEssential = true,
            });
    }

    private static void SaveInSession(HttpContext httpContext, string respondentIdString)
    {
        httpContext.Session.SetString("RespondentId", respondentIdString);
    }

    private static async Task SaveInDatabaseAsync(HttpContext httpContext, Guid respondentId)
    {
        var dbContext = httpContext.RequestServices.GetRequiredService<ApplicationDbContext>();

        dbContext.Respondents.Add(new Respondent
        {
            Id = respondentId,
            DateTime = DateTime.UtcNow,
        });

        await dbContext.SaveChangesAsync();
    }
}
