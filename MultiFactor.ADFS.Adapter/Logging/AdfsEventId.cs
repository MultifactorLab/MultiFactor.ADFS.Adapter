namespace MultiFactor.ADFS.Adapter.Logging
{
    internal static class AdfsEventId
    {
        // MultiFactor API (2xxx)
        public const ushort ApiRequestFailed = 2001;       // transport error, no response from the server
        public const ushort ApiError = 2002;               // server reachable but the API returned an error

        // Token validation / request (3xxx)
        public const ushort TokenValidationFailed = 3001;  // exception while verifying the token
        public const ushort InvalidToken = 3002;           // token verified but rejected
        public const ushort InvalidRequest = 3003;         // request without an access token
    }
    
    // Стартануло, закрылось, прошёл 2fa успешно не прошёл 2fa, 
}
