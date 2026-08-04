using System;
using Serilog;

namespace MultiFactor.ADFS.Adapter.Logging
{
    internal static class LoggerEventExtensions
    {
        // Info
        public static void AdapterStarted(this ILogger logger)
        {
            logger.ForContext(MultiFactorEventIdProvider.PropertyName, AdfsEventId.AdapterStarted)
                  .Information("Adapter started");
        }

        public static void AdapterStopping(this ILogger logger)
        {
            logger.ForContext(MultiFactorEventIdProvider.PropertyName, AdfsEventId.AdapterStopping)
                  .Information("Adapter stopping");
        }

        public static void BeginAuthentication(this ILogger logger, string login)
        {
            logger.ForContext(MultiFactorEventIdProvider.PropertyName, AdfsEventId.BeginAuthentication)
                  .Information("User: {Login}. Begin 2FA challenge", login);
        }

        public static void AuthenticationSucceeded(this ILogger logger, string login)
        {
            logger.ForContext(MultiFactorEventIdProvider.PropertyName, AdfsEventId.AuthenticationSucceeded)
                  .Information("User: {Login}. 2FA authentication succeeded", login);
        }

        // Warning
        public static void AuthenticationFailed(this ILogger logger, string login, string reason)
        {
            logger.ForContext(MultiFactorEventIdProvider.PropertyName, AdfsEventId.AuthenticationFailed)
                  .Warning("User: {Login}. 2FA authentication failed: {Reason}", login, reason);
        }

        public static void InvalidRequest(this ILogger logger)
        {
            logger.ForContext(MultiFactorEventIdProvider.PropertyName, AdfsEventId.InvalidRequest)
                  .Warning("Invalid request: no access token in proof data");
        }

        public static void BypassActivated(this ILogger logger, string login)
        {
            logger.ForContext(MultiFactorEventIdProvider.PropertyName, AdfsEventId.BypassActivated)
                  .Warning("User: {Login}. MultiFactor API unreachable, bypass activated", login);
        }

        // Error
        public static void ConfigurationError(this ILogger logger, Exception exception, string reason)
        {
            logger.ForContext(MultiFactorEventIdProvider.PropertyName, AdfsEventId.ConfigurationFailed)
                  .Error(exception, "Adapter configuration error: {Reason}", reason);
        }

        public static void ConfigurationError(this ILogger logger, string reason)
        {
            logger.ForContext(MultiFactorEventIdProvider.PropertyName, AdfsEventId.ConfigurationFailed)
                  .Error("Adapter configuration error: {Reason}", reason);
        }

        public static void ApiRequestError(this ILogger logger, Exception exception, string reason)
        {
            logger.ForContext(MultiFactorEventIdProvider.PropertyName, AdfsEventId.ApiRequestFailed)
                  .Error(exception, "MultiFactor API request failed: {Reason}", reason);
        }

        public static void ApiError(this ILogger logger, Exception exception, string reason)
        {
            logger.ForContext(MultiFactorEventIdProvider.PropertyName, AdfsEventId.ApiError)
                  .Error(exception, "MultiFactor API error: {Reason}", reason);
        }

        public static void TokenValidationError(this ILogger logger, Exception exception)
        {
            logger.ForContext(MultiFactorEventIdProvider.PropertyName, AdfsEventId.TokenValidationFailed)
                  .Error(exception, "Token validation failed");
        }

        public static void AdapterError(this ILogger logger, Exception exception)
        {
            logger.ForContext(MultiFactorEventIdProvider.PropertyName, AdfsEventId.AdapterError)
                  .Error(exception, "Unhandled authentication error");
        }
    }
}
