using System;
using System.IO;
using MultiFactor.ADFS.Adapter.Services;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.Syslog;

namespace MultiFactor.ADFS.Adapter.Logging
{
    public static class SerilogLoggerFactory
    {
        private const string EventLogName = "Application";
        private const string EventLogSource = "MultiFactor";
        private const string DefaultSyslogAppName = "multifactor-adfs";

        private const string FileOutputTemplate =
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{EventId}] {Message:lj}{NewLine}{Exception}";
        private const string EventLogOutputTemplate =
            "{Message:lj}{NewLine}{Exception}";

        public static ILogger CreateLogger(MultiFactorConfiguration config)
        {
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Is(ParseLevel(config?.LoggingLevel));

            ConfigureEventLog(loggerConfiguration);
            ConfigureFile(loggerConfiguration, config);

            Exception syslogError = null;
            if (!string.IsNullOrWhiteSpace(config?.SyslogServer))
            {
                try
                {
                    ConfigureSyslog(loggerConfiguration, config);
                }
                catch (Exception ex)
                {
                    syslogError = ex;
                }
            }

            var logger = loggerConfiguration.CreateLogger();
            if (syslogError != null)
            {
                logger.Warning(syslogError,
                    "Failed to configure syslog sink for server {Server}, syslog will be disabled",
                    config.SyslogServer);
            }

            return logger;
        }

        private static void ConfigureEventLog(LoggerConfiguration loggerConfiguration)
        {
            loggerConfiguration.WriteTo.EventLog(
                source: EventLogSource,
                logName: EventLogName,
                manageEventSource: false,
                outputTemplate: EventLogOutputTemplate,
                eventIdProvider: new MultiFactorEventIdProvider());
        }

        private static void ConfigureFile(LoggerConfiguration loggerConfiguration, MultiFactorConfiguration config)
        {
            var logFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                "MultiFactor", "Logs", "multifactor-adfs-.log");

            if (IsJson(config?.LoggingFormat))
            {
                loggerConfiguration.WriteTo.File(
                    new JsonFormatter(),
                    path: logFile,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: null, //??
                    flushToDiskInterval: TimeSpan.FromSeconds(1),
                    // ADFS hosts the adapter in multiple worker processes — shared mode prevents each process from creating its own log file
                    shared: true);
                return;
            }

            var template = string.IsNullOrWhiteSpace(config?.FileLogOutputTemplate)
                ? FileOutputTemplate
                : config.FileLogOutputTemplate;

            loggerConfiguration.WriteTo.File(
                path: logFile,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: null, //?
                flushToDiskInterval: TimeSpan.FromSeconds(1),
                outputTemplate: template,
                // ADFS hosts the adapter in multiple worker processes — shared mode prevents each process from creating its own log file
                shared: true);
        }

        private static void ConfigureSyslog(LoggerConfiguration loggerConfiguration, MultiFactorConfiguration config)
        {
            var format = ParseEnum(config.SyslogFormat, SyslogFormat.RFC5424);
            var facility = ParseEnum(config.SyslogFacility, Facility.Auth);
            var framer = ParseEnum(config.SyslogFramer, FramingType.OCTET_COUNTING);
            var appName = string.IsNullOrWhiteSpace(config.SyslogAppName) ? DefaultSyslogAppName : config.SyslogAppName;
            var template = string.IsNullOrWhiteSpace(config.SyslogOutputTemplate) ? null : config.SyslogOutputTemplate;
            var useTls = config.SyslogUseTls ?? false;

            var uri = new Uri(config.SyslogServer);

            switch (uri.Scheme.ToLowerInvariant())
            {
                case "udp":
                    loggerConfiguration.WriteTo.UdpSyslog(
                        host: uri.Host,
                        port: uri.Port,
                        appName: appName,
                        format: format,
                        facility: facility,
                        outputTemplate: template);
                    break;
                case "tcp":
                    loggerConfiguration.WriteTo.TcpSyslog(
                        host: uri.Host,
                        port: uri.Port,
                        appName: appName,
                        framingType: framer,
                        format: format,
                        facility: facility,
                        useTls: useTls,
                        outputTemplate: template);
                    break;
                default:
                    throw new ArgumentException($"Unsupported syslog scheme '{uri.Scheme}'. Use udp:// or tcp://");
            }
        }

        private static LogEventLevel ParseLevel(string level)
        {
            switch ((level ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "verbose": return LogEventLevel.Verbose;
                case "debug": return LogEventLevel.Debug;
                case "info": return LogEventLevel.Information;
                case "warn": return LogEventLevel.Warning;
                case "error": return LogEventLevel.Error;
                default: return LogEventLevel.Information;
            }
        }

        private static TEnum ParseEnum<TEnum>(string value, TEnum fallback) where TEnum : struct
        {
            return Enum.TryParse(value, true, out TEnum result) ? result : fallback;
        }

        private static bool IsJson(string format)
        {
            return string.Equals(format, "json", StringComparison.OrdinalIgnoreCase);
        }
    }
}
