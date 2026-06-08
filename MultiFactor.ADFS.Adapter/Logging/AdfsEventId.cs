namespace MultiFactor.ADFS.Adapter.Logging
{
    internal static class AdfsEventId
    {
        // Defaults — used when no specific event ID is set
        public const ushort DefaultInfo = 10000;
        public const ushort DefaultWarning = 20000;
        public const ushort DefaultError = 30000;

        // Info (10xxx)
        public const ushort AdapterStarted = DefaultInfo + 1; // adapter loaded and ready to handle requests
        public const ushort AdapterStopping = DefaultInfo + 2; // ADFS requested adapter unload
        public const ushort BeginAuthentication = DefaultInfo + 3; // user entered 2FA flow
        public const ushort AuthenticationSucceeded = DefaultInfo + 4; // token valid, claims issued

        // Warning (20xxx)
        public const ushort AuthenticationFailed = DefaultWarning + 1; // 2FA not passed: token rejected or identity mismatch
        public const ushort InvalidRequest = DefaultWarning + 2; // request arrived without an access token
        public const ushort BypassActivated = DefaultWarning + 3; // API unreachable, falling back to bypass token

        // Error (30xxx)
        public const ushort ApiRequestFailed = DefaultError + 1; // transport error, no response from MultiFactor API
        public const ushort ApiError = DefaultError + 2; // API reachable but returned an error response
        public const ushort TokenValidationFailed = DefaultError + 3; // exception thrown during token verification
        public const ushort AdapterError = DefaultError + 4; // unhandled error reported by ADFS pipeline
    }
}