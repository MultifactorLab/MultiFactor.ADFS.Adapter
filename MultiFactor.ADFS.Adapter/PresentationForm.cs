using Microsoft.IdentityServer.Web.Authentication.External;
using System;

namespace MultiFactor.ADFS.Adapter
{
    public class PresentationForm : IAdapterPresentationForm
    {
        private string _accessPageUrl;

        public PresentationForm(string accessPageUrl)
        {
            _accessPageUrl = accessPageUrl ?? throw new ArgumentNullException(nameof(accessPageUrl));
        }

        public string GetFormHtml(int lcid)
        {
            var htmlTemplate = Resources.MfaFormHtml;
            return htmlTemplate.Replace("%AccessPageUrl%", _accessPageUrl);
        }

        public string GetFormPreRenderHtml(int lcid)
        {
            return null;
        }

        public string GetPageTitle(int lcid)
        {
            return "MultiFactor";
        }
    }
}
