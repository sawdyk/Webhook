using IMTOWEBHOOK.Logs;
using IMTOWEBHOOK.RequestModels;
using IMTOWEBHOOK.ResponseModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace IMTOWEBHOOK.Helpers
{
    public class HttpClientConfig
    {
        private static CustomLogger logger;
        private string AppRootDirectory;

        public HttpClientConfig()
        {
            AppRootDirectory = HttpRuntime.AppDomainAppPath;
            logger = new CustomLogger(AppRootDirectory);
        }

        public static async Task<HttpResponseMessage> PostRequest(MailRequestModel obj)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);

                    string BaseUrl = ConfigurationManager.AppSettings["BaseBankApiUrl"];

                    string emailEndpoint = ConfigurationManager.AppSettings["EmailEndpoint"];

                    string fullUrl = BaseUrl + emailEndpoint;

                    StringContent jsonContent = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");

                    httpClient.BaseAddress = new Uri(BaseUrl);
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var apiResponse = await httpClient.PostAsync(fullUrl, jsonContent);
                    //var stringResponse = await apiResponse.Content.ReadAsStringAsync();
                    //var mailResponse = JsonConvert.DeserializeObject<MailResponseModel>(stringResponse);

                    return apiResponse;
                }
            }
            catch (Exception ex)
            {
                var errorLogInfo = new ErrorLogInfo()
                {
                    ExceptionMessage = ex.Message,
                    InnerException = ex.InnerException == null ? "None" : ex.InnerException.Message,
                    StackTrace = ex.StackTrace,
                    Time = DateTime.Now.ToString()
                };

                logger.LogErrorToTextFile(errorLogInfo); //

                return new HttpResponseMessage();
            }
        }
    }
}