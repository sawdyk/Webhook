using IMTOWEBHOOK.Logs;
using IMTOWEBHOOK.RequestModels;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace IMTOWEBHOOK.Helpers
{
    internal class MailSender
    {
        private static CustomLogger logger;
        private string AppRootDirectory;

        public MailSender()
        {
            AppRootDirectory = HttpRuntime.AppDomainAppPath;
            logger = new CustomLogger(AppRootDirectory);
        }
        public static bool Sendmail(string Message)
        {
            ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);

            try
            {
                bool isSent = false;

                //get email template from path and use it
                string fileName = "EmailTemplate\\emailtemp.html";
                string folder = Path.GetDirectoryName(HttpRuntime.AppDomainAppPath);
                string filePath = Path.Combine(folder, fileName);

                var sr = new StreamReader(File.OpenRead(filePath));
                var sb = new StringBuilder();
                var line = sr.ReadToEnd();
                sb.Append(line);
                var emailBody = sb.ToString();
                var curYear = DateTime.Now.Year;

                emailBody = emailBody.Replace("{emailBody}", Message);
                emailBody = emailBody.Replace("{currentYear}", curYear.ToString());

                //API and Email Configuration 
                string sender = ConfigurationManager.AppSettings["EmailSender"];
                string from = ConfigurationManager.AppSettings["EmailFrom"];
                string cc = ConfigurationManager.AppSettings["Emailcc"];
                string email = ConfigurationManager.AppSettings["EmailTo"];
                string Subject = ConfigurationManager.AppSettings["EmailSubject"];

                string baseBankApiUrl = ConfigurationManager.AppSettings["BaseBankApiUrl"];
                string emailEndpoint = ConfigurationManager.AppSettings["EmailEndpoint"];

                var mailModel = new MailRequestModel()
                {
                    sender = sender,
                    from = from,
                    to = email,
                    cc = cc,
                    bcc = "",
                    subject = Subject,
                    body = emailBody,                   
                    // attachments = attachment
                };

                RestClient restClient = new RestClient(baseBankApiUrl);
                RestRequest restRequest = new RestRequest(emailEndpoint, Method.POST);
                restRequest.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(mailModel), ParameterType.RequestBody);
                restRequest.RequestFormat = DataFormat.Json;
                IRestResponse restResponse = restClient.Execute((IRestRequest)restRequest);
                if (restResponse.IsSuccessful)
                {
                    isSent = true;
                }

                return isSent;
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

                return false;
            }
        }
    }
}