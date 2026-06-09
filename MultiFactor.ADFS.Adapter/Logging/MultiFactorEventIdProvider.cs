using Serilog.Events;
using Serilog.Sinks.EventLog;

namespace MultiFactor.ADFS.Adapter.Logging
{
    internal class MultiFactorEventIdProvider : IEventIdProvider
    {
        public const string PropertyName = "EventId";

        public ushort ComputeEventId(LogEvent logEvent)
        {
            if (logEvent.Properties.TryGetValue(PropertyName, out var value)
                && value is ScalarValue scalar
                && scalar.Value != null
                && ushort.TryParse(scalar.Value.ToString(), out var id))
            {
                return id;
            }

            switch (logEvent.Level)
            {
                case LogEventLevel.Information:
                    return AdfsEventId.DefaultInfo;
                case LogEventLevel.Warning:
                    return AdfsEventId.DefaultWarning;
                case LogEventLevel.Error:
                case LogEventLevel.Fatal:
                    return AdfsEventId.DefaultError;
                default:
                    return 0;
            }
        }
    }
}
