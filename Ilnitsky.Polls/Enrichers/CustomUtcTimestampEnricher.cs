using Serilog.Core;
using Serilog.Events;

namespace Ilnitsky.Polls.Enrichers;

public class CustomUtcTimestampEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var utcTimeString = logEvent.Timestamp.UtcDateTime.ToString("HH:mm:ss.fff") + "Z";

        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty("CustomUtcTimestamp", utcTimeString));
    }
}
