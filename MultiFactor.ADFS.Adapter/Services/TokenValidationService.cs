using System;
using System.Collections.Generic;
using System.Text;

namespace MultiFactor.ADFS.Adapter.Services
{
    /// <summary>
    /// Service to load public key and verify token signature, issuer and expiration date
    /// </summary>
    public class TokenValidationService
    {
        private MultiFactorConfiguration _configuration;

        public TokenValidationService(MultiFactorConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
        /// <summary>
        /// Generate JWT when Bypass mode. 
        /// </summary>
        public string GenerateBypassToken(string login)
        {
            var key = Encoding.UTF8.GetBytes(_configuration.ApiSecret);
            var origtime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var exptime = DateTime.UtcNow.AddMinutes(20);
            var body = Util.Base64UrlEncode(Encoding.UTF8.GetBytes("{" + $"\"aud\":\"{_configuration.ApiKey}\",\"exp\":{(long)(exptime - origtime).TotalSeconds},\"sub\":\"{login}\"" + "}"));
            var head = Util.Base64UrlEncode(Encoding.UTF8.GetBytes("{\"typ\":\"JWT\",\"alg\":\"HS256\"}"));
            var message = $"{head}.{body}";
            var sign = Util.Base64UrlEncode(Util.HMACSHA256(key, Encoding.UTF8.GetBytes(message)));
            return $"bypass.{message}.{sign}";

        }
        /// <summary>
        /// Verify JWT
        /// </summary>
        public string VerifyToken(string jwt)
        {
            //https://multifactor.ru/docs/integration/

            if (string.IsNullOrEmpty(jwt))
            {
                throw new ArgumentNullException(nameof(jwt));
            }

            var parts = jwt.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            var bypass = false;
            if (parts[0] == "bypass") bypass = true;
            var head = parts[bypass?1:0];
            var body = parts[bypass?2:1];
            var sign = parts[bypass?3:2];

            //validate JwtHS256 token signature
            var key = Encoding.UTF8.GetBytes(_configuration.ApiSecret);
            var message = Encoding.UTF8.GetBytes($"{head}.{body}");

            var computedSign = Util.Base64UrlEncode(Util.HMACSHA256(key, message));

            if (computedSign != sign)
            {
                throw new Exception("Invalid token signature");
            }

            var decodedBody = Encoding.UTF8.GetString(Util.Base64UrlDecode(body));
            var json = Util.JsonDeserialize<Dictionary<string, object>>(decodedBody);

            //validate audience
            var aud = json["aud"] as string;
            if (aud != _configuration.ApiKey)
            {
                throw new Exception("Invalid token audience");
            }

            //validate expiration date
            var exp = Convert.ToInt64(json["exp"]);
            if (Util.UnixTimeStampToDateTime(exp) < DateTime.UtcNow)
            {
                throw new Exception("Expired token");
            }

            //identity
            var sub = json["sub"] as string;
            if (string.IsNullOrEmpty(sub))
            {
                throw new Exception("Name ID not found");
            }

            return sub;
        }

        /// <summary>
        /// Verify JWT safe
        /// </summary>
        public bool TryVerifyToken(string jwt, out string identity)
        {
            try
            {
                identity = VerifyToken(jwt);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to parse token: {ex.Message}, {ex}");
                identity = null;
                return false;
            }
        }
    }
}
