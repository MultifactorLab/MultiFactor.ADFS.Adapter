using System;
using System.Globalization;
using System.IO;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;

namespace MultiFactor.ADFS.Adapter.Logging
{
    internal sealed class CustomCompactJsonFormatter : ITextFormatter
    {
        private readonly string _timestampFormat;
        private readonly JsonValueFormatter _valueFormatter;

        public CustomCompactJsonFormatter(string timestampFormat)
        {
            if (string.IsNullOrWhiteSpace(timestampFormat))
                throw new ArgumentNullException(nameof(timestampFormat));

            _timestampFormat = timestampFormat;
            _valueFormatter = new JsonValueFormatter(typeTagName: "$type");
        }

        public void Format(LogEvent logEvent, TextWriter output)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));
            if (output == null) throw new ArgumentNullException(nameof(output));

            output.Write("{\"@t\":\"");
            output.Write(logEvent.Timestamp.ToString(_timestampFormat, CultureInfo.InvariantCulture));
            output.Write("\",\"@m\":");
            var message = logEvent.MessageTemplate.Render(logEvent.Properties, CultureInfo.InvariantCulture);
            JsonValueFormatter.WriteQuotedJsonString(message, output);
            output.Write(",\"@mt\":");
            JsonValueFormatter.WriteQuotedJsonString(logEvent.MessageTemplate.Text, output);

            if (logEvent.Level != LogEventLevel.Information)
            {
                output.Write(",\"@l\":\"");
                output.Write(logEvent.Level);
                output.Write('\"');
            }

            if (logEvent.Exception != null)
            {
                output.Write(",\"@x\":");
                JsonValueFormatter.WriteQuotedJsonString(logEvent.Exception.ToString(), output);
            }

            foreach (var property in logEvent.Properties)
            {
                var name = property.Key;
                if (name.Length > 0 && name[0] == '@')
                    name = '@' + name;

                output.Write(',');
                JsonValueFormatter.WriteQuotedJsonString(name, output);
                output.Write(':');
                _valueFormatter.Format(property.Value, output);
            }

            output.Write('}');
            output.WriteLine();
        }
    }
}
