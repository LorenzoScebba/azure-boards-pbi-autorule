using Serilog.Core;
using Serilog.Events;

namespace azure_boards_pbi_autorule.Configurations
{
    public class SerilogContextRemover : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.RemovePropertyIfPresent("ActionId");
            logEvent.RemovePropertyIfPresent("ActionName");
            logEvent.RemovePropertyIfPresent("RequestId");
            logEvent.RemovePropertyIfPresent("RequestPath");
            logEvent.RemovePropertyIfPresent("SpanId");
            logEvent.RemovePropertyIfPresent("TraceId");
            logEvent.RemovePropertyIfPresent("ParentId");
            logEvent.RemovePropertyIfPresent("ConnectionId");
        }
    }
}