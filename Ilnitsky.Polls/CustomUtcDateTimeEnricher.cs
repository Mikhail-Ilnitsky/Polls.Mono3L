using Serilog.Core;
using Serilog.Events;

namespace Ilnitsky.Polls;

public class CustomUtcDateTimeEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var utcDateTimeString = logEvent.Timestamp.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff") + "Z";

        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty("CustomUtcDateTime", utcDateTimeString));
    }
}
