using System;
using System.Net;
using System.Text;

namespace MultiFactor.ADFS.Adapter.Services
{
    /// <summary>
    /// Service to interact with MultiFactor API
    /// </summary>
    public class MultiFactorApiClient
    {
        private MultiFactorConfiguration _configuration;

        public MultiFactorApiClient(MultiFactorConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public string CreateRequest(string login, string target, string postbackUrl)
        {
            try
            {
                //make sure we can communicate securely
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                //payload
                var json = Util.JsonSerialize(new
                {
                    Identity = login,
                    Callback = new
                    {
                        Action = postbackUrl,
                        Target = target
                    },
                });

                var requestData = Encoding.UTF8.GetBytes(json);
                byte[] responseData = null;

                //basic authorization
                var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(_configuration.ApiKey + ":" + _configuration.ApiSecret));


                using (var web = new WebClient())
                {
                    web.Headers.Add("Content-Type", "application/json");
                    web.Headers.Add("Authorization", "Basic " + auth);

                    if (!string.IsNullOrEmpty(_configuration.ApiProxy))
                    {
                        web.Proxy = new WebProxy(_configuration.ApiProxy);
                    }

                    responseData = web.UploadData(_configuration.ApiUrl + "/access/requests", "POST", requestData);
                }

                json = Encoding.UTF8.GetString(responseData);


                var response = Util.JsonDeserialize<MultiFactorWebResponse<MultiFactorAccessPage>>(json);

                if (!response.Success) throw new Exception(response.Message);

                return response.Model.Url;
            }
            catch (Exception ex)
            {
                Logger.Error("MultiFactor API error: " + ex.Message);
                if (_configuration.Bypass) return "bypass";
                throw new Exception("MultiFactor API error: " + ex.Message);
            }
        }
    }

    public class MultiFactorWebResponse<TModel>
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public TModel Model { get; set; }
    }

    public class MultiFactorAccessPage
    {
        public string Url { get; set; }
    }

}
