using Microsoft.IdentityServer.Web.Authentication.External;
using System.Collections.Generic;
using System.Globalization;

namespace MultiFactor.ADFS.Adapter
{
    public class Metadata : IAuthenticationAdapterMetadata
    {
        public string AdminName => "MultiFactor";

        public string[] AuthenticationMethods => new[] { Constants.AUTH_CLAIM };

        public int[] AvailableLcids => new[] { new CultureInfo("ru").LCID, new CultureInfo("en-us").LCID };

        public Dictionary<int, string> Descriptions => FriendlyNames;

        public Dictionary<int, string> FriendlyNames
        {
            get
            {
                return new Dictionary<int, string>
                {
                    { new CultureInfo("ru").LCID, "Многофакторная аутентификация" },
                    { new CultureInfo("en-us").LCID, "MultiFactor Authentication" }
                };
            }
        }

        public string[] IdentityClaims => new[] { "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn" };

        public bool RequiresIdentity => true;
    }
}