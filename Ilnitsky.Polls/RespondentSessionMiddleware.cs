using Ilnitsky.Polls.BusinessLogic;
using Ilnitsky.Polls.DataAccess;
using Ilnitsky.Polls.DataAccess.Entities.Answers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ilnitsky.Polls;

public class RespondentSessionMiddleware(RequestDelegate _next)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (!httpContext.Session.Keys.Contains("RespondentSessionId"))
        {
            var respondentSessionId = GuidHelper.CreateGuidV7();

            httpContext.Session.SetString("RespondentSessionId", respondentSessionId.ToString());

            var respondentIdString = httpContext.Session.GetString("RespondentId");
            var respondentId = Guid.Parse(respondentIdString);

            var dbContext = httpContext.RequestServices.GetRequiredService<ApplicationDbContext>();

            dbContext.RespondentSessions.Add(new RespondentSession
            {
                Id = respondentSessionId,
                DateTime = DateTime.UtcNow,
                RespondentId = respondentId,
                IsMobile = httpContext.Request.Headers["sec-ch-ua-mobile"] == "?1",
                RemoteIpAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                UserAgent = httpContext.Request.Headers.UserAgent.ToString(),
                AcceptLanguage = httpContext.Request.Headers.AcceptLanguage.ToString(),
                Platform = httpContext.Request.Headers["sec-ch-ua-platform"].ToString(),
                Brand = httpContext.Request.Headers["sec-ch-ua"].ToString()
            });

            await dbContext.SaveChangesAsync();
        }

        await _next.Invoke(httpContext);
    }
}
