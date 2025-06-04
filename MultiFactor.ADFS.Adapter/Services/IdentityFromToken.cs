namespace MultiFactor.ADFS.Adapter.Services
{
    internal class IdentityFromToken
    {
        /// <summary>
        /// User identity if MF
        /// </summary>
        public string MfIdentity { get; set; }

        /// <summary>
        /// User identity in adfs
        /// </summary>
        public string AdfsIdentity { get; set; }
    }
}
