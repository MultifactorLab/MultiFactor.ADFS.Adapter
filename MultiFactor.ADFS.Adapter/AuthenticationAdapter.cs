using Microsoft.IdentityServer.Web.Authentication.External;
using MultiFactor.ADFS.Adapter.Logging;
using MultiFactor.ADFS.Adapter.Services;
using Serilog;
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
        private ILogger _logger = SerilogLoggerFactory.CreateLogger(null);

        public IAuthenticationAdapterMetadata Metadata => new Metadata();

        public AuthenticationAdapter()
        {
            _logger.AdapterStarted();
        }

        public IAdapterPresentation BeginAuthentication(Claim identityClaim, HttpListenerRequest request, IAuthenticationContext context)
        {
            try
            {
                var login = identityClaim.Value;

                _logger.BeginAuthentication(login);

                // save current username in auth context
                context.Data.Add(Constants.AUTH_CONTEXT_IDENTITY, login);

                var mfaUrl = CreateAccessRequest(login);
                if (mfaUrl == "bypass")
                {
                    var tokenValidationService = new TokenValidationService(_configuration, _logger);
                    mfaUrl = tokenValidationService.GenerateBypassToken(login);

                    _logger.BypassActivated(login);
                }
                return new PresentationForm(mfaUrl);
            }
            catch (Exception ex)
            {
                _logger.AdapterError(ex);
                throw;
            }
        }

        public bool IsAvailableForUser(Claim identityClaim, IAuthenticationContext context)
        {
            return true;
        }

        public void OnAuthenticationPipelineLoad(IAuthenticationMethodConfigData configData)
        {
            if (configData?.Data != null)
            {
                try
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
                        if (!bool.TryParse(bypassElement?.Attributes["value"].Value, out bool bypass)) bypass = true;

                        _configuration = new MultiFactorConfiguration
                        {
                            ApiUrl = apiUrlElement.Attributes["value"].Value,
                            ApiKey = apiKeyElement.Attributes["value"].Value,
                            ApiSecret = apiSecretElement.Attributes["value"].Value,
                            ApiProxy = apiProxyElement?.Attributes["value"].Value, //optional
                            Bypass = bypass,

                            LoggingLevel = GetValue(appSettings, "logging-level"),
                            LoggingFormat = GetValue(appSettings, "logging-format"),
                            SyslogServer = GetValue(appSettings, "syslog-server"),
                            SyslogFormat = GetValue(appSettings, "syslog-format"),
                            SyslogFacility = GetValue(appSettings, "syslog-facility"),
                            SyslogAppName = GetValue(appSettings, "syslog-app-name"),
                            SyslogFramer = GetValue(appSettings, "syslog-framer"),
                            SyslogUseTls = ParseNullableBool(GetValue(appSettings, "syslog-use-tls")),
                            SyslogOutputTemplate = GetValue(appSettings, "syslog-output-template"),
                            FileLogOutputTemplate = GetValue(appSettings, "file-log-output-template")
                        };
                    }

                    _logger = SerilogLoggerFactory.CreateLogger(_configuration);
                    _logger.Debug("Configuration loaded. {Configuration}", _configuration);
                }
                catch (Exception ex)
                {
                    _logger.ConfigurationError(ex, "Failed to parse adapter configuration");
                    throw;
                }
            }
            else
            {
                _logger.ConfigurationError("No configuration data provided by ADFS");
                throw new Exception("Configuration error");
            }
        }

        public void OnAuthenticationPipelineUnload()
        {
            _logger?.AdapterStopping();
        }

        public IAdapterPresentation OnError(HttpListenerRequest request, ExternalAuthenticationException ex)
        {
            throw new UnauthorizedAccessException();
        }

        public IAdapterPresentation TryEndAuthentication(IAuthenticationContext context, IProofData proofData, HttpListenerRequest request, out Claim[] claims)
        {
            try
            {
                if (proofData?.Properties?.ContainsKey("AccessToken") == true)
                {
                    var accessKey = proofData.Properties["AccessToken"] as string;

                    var tokenValidationService = new TokenValidationService(_configuration, _logger);

                    var adfsUsername = context.Data[Constants.AUTH_CONTEXT_IDENTITY] as string;
                    if (adfsUsername == null)
                    {
                        _logger.AdapterError(new InvalidOperationException("User identity not found in authentication context"));
                        throw new ExternalAuthenticationException("Can't get username from context", context);
                    }

                    if (tokenValidationService.TryVerifyToken(accessKey, adfsUsername))
                    {
                        claims = new[] { new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/authenticationmethod", Constants.AUTH_CLAIM) };
                        _logger.AuthenticationSucceeded(adfsUsername);
                        return null;
                    }
                    else
                    {
                        const string reason = "Invalid token";
                        _logger.AuthenticationFailed(adfsUsername, reason);
                        throw new ExternalAuthenticationException(reason, context);
                    }
                }

                _logger.InvalidRequest();
                throw new ExternalAuthenticationException("Invalid request", context);
            }
            catch (ExternalAuthenticationException)
            {
                // intentional, already logged above
                throw;
            }
            catch (Exception ex)
            {
                _logger.AdapterError(ex);
                throw;
            }
        }

        private static string GetValue(XmlNode appSettings, string key)
        {
            var element = (XmlElement)appSettings.SelectSingleNode($"//add[@key='{key}']");
            return element?.Attributes["value"]?.Value;
        }

        private static bool? ParseNullableBool(string value)
        {
            return bool.TryParse(value, out var result) ? result : (bool?)null;
        }

        private string CreateAccessRequest(string identity)
        {
            // call postMessage with accessToken from iframe to parent window instead of submit
            var postBack = "javascript:window.parent.postMessage($`AccessToken`,'*')";

            var client = new MultiFactorApiClient(_configuration, _logger);
            return client.CreateRequest(identity, "_self", postBack);
        }
    }
}
