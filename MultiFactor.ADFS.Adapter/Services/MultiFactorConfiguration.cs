namespace MultiFactor.ADFS.Adapter.Services
{
    public class MultiFactorConfiguration
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string ApiUrl { get; set; }
        public string ApiProxy { get; set; }
        public bool Bypass { get; set; }

        // logging
        public string LoggingLevel { get; set; }
        public string LoggingFormat { get; set; }
        public string SyslogServer { get; set; }
        public string SyslogFormat { get; set; }
        public string SyslogFacility { get; set; }
        public string SyslogAppName { get; set; }
        public string SyslogFramer { get; set; }
        public bool? SyslogUseTls { get; set; }
        public string SyslogOutputTemplate { get; set; }
        public string FileLogOutputTemplate { get; set; }
        public long? LogFileMaxSizeBytes { get; set; }

        public override string ToString()
        {
            return $"ApiUrl={ApiUrl}, ApiKey={ApiKey}, ApiProxy={ApiProxy}, Bypass={Bypass}";
        }
    }
}