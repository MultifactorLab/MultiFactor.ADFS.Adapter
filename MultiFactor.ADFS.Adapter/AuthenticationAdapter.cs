using Microsoft.IdentityServer.Web.Authentication.External;
using MultiFactor.ADFS.Adapter.Services;
using System;
using System.Net;
using Claim = System.Security.Claims.Claim;
using System.Xml;
using System.IO;

namespace MultiFactor.ADFS.Adapter
{
    public class AuthenticationAdapter : IAuthenticationAdapter
    {
        private MultiFactorConfiguration _configuration;

        public IAuthenticationAdapterMetadata Metadata => new Metadata();

        public IAdapterPresentation BeginAuthentication(Claim identityClaim, HttpListenerRequest request, IAuthenticationContext context)
        {
            var login = identityClaim.Value;

            //save current username in auth context
            context.Data.Add(Constants.AUTH_CONTEXT_IDENTITY, login);

            var mfaUrl = CreateAccessRequest(login);
            if (mfaUrl == "bypass")
            {
                var tokenValidationService = new TokenValidationService(_configuration);
                mfaUrl = tokenValidationService.GenerateBypassToken(login);
            }
            return new PresentationForm(mfaUrl);
        }

        public bool IsAvailableForUser(Claim identityClaim, IAuthenticationContext context)
        {
            return true;
        }

        public void OnAuthenticationPipelineLoad(IAuthenticationMethodConfigData configData)
        {
            //load configuration from xml file
            if (configData?.Data != null)
            {
                using (var sr = new StreamReader(configData.Data))
                {
                    var text = sr.ReadToEnd();
                    var doc = new XmlDocument();
                    doc.LoadXml(text);

                    var appSettings = doc.SelectSingleNode("//appSettings");
                    var apiUrlElement = (XmlElement)appSettings.SelectSingleNode("//add[@key='multifactor-api-url']");
                    var apiKeyElement = (XmlElement)appSettings.SelectSingleNode("//add[@key='multifactor-api-key']");
                    var apiSecretElement = (XmlElement)appSettings.SelectSingleNode("//add[@key='multifactor-api-secret']");
                    var apiProxyElement = (XmlElement)appSettings.SelectSingleNode("//add[@key='multifactor-api-proxy']");
                    var bypassElement = (XmlElement)appSettings.SelectSingleNode("//add[@key='bypass-second-factor-when-api-unreachable']");
                    bool bypass = true;
                    if(bypassElement!=null)
                        bool.TryParse(bypassElement.Attributes["value"].Value, out bypass);

                    _configuration = new MultiFactorConfiguration
                    {
                        ApiUrl = apiUrlElement.Attributes["value"].Value,
                        ApiKey = apiKeyElement.Attributes["value"].Value,
                        ApiSecret = apiSecretElement.Attributes["value"].Value,
                        ApiProxy = apiProxyElement?.Attributes["value"].Value, //optional
                        Bypass = bypass
                    };
                }
            }
            else
            {
                Logger.Error("Can't load configuration, check ConfigurationFilePath");
                throw new Exception("Configuration error");
            }
        }

        public void OnAuthenticationPipelineUnload()
        {
        }

        public IAdapterPresentation OnError(HttpListenerRequest request, ExternalAuthenticationException ex)
        {
            throw new UnauthorizedAccessException();
        }

        public IAdapterPresentation TryEndAuthentication(IAuthenticationContext context, IProofData proofData, HttpListenerRequest request, out Claim[] claims)
        {
            if (proofData?.Properties?.ContainsKey("AccessToken") == true)
            {
                //get jwt from form
                var accessKey = proofData.Properties["AccessToken"] as string;
                
                var tokenValidationService = new TokenValidationService(_configuration);

                //validate jwt
                if (tokenValidationService.TryVerifyToken(accessKey, out var userName))
                {
                    var contextUserName = context.Data[Constants.AUTH_CONTEXT_IDENTITY] as string;
                    if (contextUserName == null)
                    {
                        throw new ExternalAuthenticationException("Can't get username from context", context);
                    }

                    //check username
                    if (Util.CanonicalizeUserName(contextUserName) == Util.CanonicalizeUserName(userName))
                    {
                        //ok
                        claims = new[] { new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", Constants.AUTH_CLAIM) };
                        // null == authentication succeeded.
                        return null;
                    }
                } 
                else
                {
                    Logger.Warn("Invalid token");
                    throw new ExternalAuthenticationException("Invalid token", context);
                }
            }
         
            Logger.Warn("Invalid request");
            throw new ExternalAuthenticationException("Invalid request", context);
        }

        private string CreateAccessRequest(string identity)
        {
            //call postMessage with accessToken from iframe to parent window instead of submit
            var postBack = "javascript:window.parent.postMessage($`AccessToken`,'*')";

            var client = new MultiFactorApiClient(_configuration);
            return client.CreateRequest(identity, "_self", postBack);
        }
    }
}