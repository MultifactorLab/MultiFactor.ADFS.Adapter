using System;
using Serilog;

namespace MultiFactor.ADFS.Adapter.Logging
{
    internal static class LoggerEventExtensions
    {
        public static void ApiRequestFailed(this ILogger logger, Exception exception, string reason)
        {
            logger.ForContext(MultiFactorEventIdProvider.PropertyName, AdfsEventId.ApiRequestFailed)
                  .Error(exception, "MultiFactor API request failed: {Reason}", reason);
        }

        public static void ApiError(this ILogger logger, Exception exception, string reason)
        {
            logger.ForContext(MultiFactorEventIdProvider.PropertyName, AdfsEventId.ApiError)
                  .Error(exception, "MultiFactor API error: {Reason}", reason);
        }

        public static void TokenValidationFailed(this ILogger logger, Exception exception)
        {
            logger.ForContext(MultiFactorEventIdProvider.PropertyName, AdfsEventId.TokenValidationFailed)
                  .Error(exception, "Failed to validate token");
        }

        public static void InvalidToken(this ILogger logger)
        {
            logger.ForContext(MultiFactorEventIdProvider.PropertyName, AdfsEventId.InvalidToken)
                  .Warning("Invalid token");
        }

        public static void InvalidRequest(this ILogger logger)
        {
            logger.ForContext(MultiFactorEventIdProvider.PropertyName, AdfsEventId.InvalidRequest)
                  .Warning("Invalid request");
        }
    }
}
