using IMTOWEBHOOK.Helpers;
using IMTOWEBHOOK.Logs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using static IMTOWEBHOOK.Models.GloCurrencyModels;

namespace IMTOWEBHOOK.Controllers
{
    public class GloCurrencyController : ApiController
    {
        private CustomLogger logger;
        private string AppRootDirectory;

        public GloCurrencyController()
        {
            AppRootDirectory = HttpRuntime.AppDomainAppPath;
            logger = new CustomLogger(AppRootDirectory);
        }

        [HttpPost]
        [Route("api/glocurrency/webhook")]
        public async Task<HttpResponseMessage> webhook()
        {
            ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);

            try
            {
                //the incoming event from GloCurrency
                var gloCurrencyJsonResponse = new StreamReader(HttpContext.Current.Request.InputStream).ReadToEnd();

                //deserialize
                var jsonObj = JsonConvert.DeserializeObject<WebhookData>(gloCurrencyJsonResponse);

                //headers
                var headers = HttpContext.Current.Request.Headers;

                //Get Headers
                string header_authorization_signature = headers.GetValues("authorization-signature").ToList()[0];
                string header_authorization_nonce = headers.GetValues("authorization-nonce").ToList()[0];

                if (!string.IsNullOrEmpty(header_authorization_signature) && !string.IsNullOrEmpty(header_authorization_nonce))
                {
                    string httpVerb = ConfigHelpers.GloHttpVerb;
                    string requestUrl = ConfigurationManager.AppSettings["GloCurrencyWebHookUrl"].ToString();

                    string encryptedReqBody = GloCurrencyRequestEncryptor.postRequestEncrypt(gloCurrencyJsonResponse);

                    string gen_auth_signature = GloCurrencyAuthorization.authorization_signature(header_authorization_nonce.Trim(), httpVerb, requestUrl, encryptedReqBody);

                    if (header_authorization_signature.Equals(gen_auth_signature)) //check if the Header Auth Sugnature is the same as the Generated Auth Signature 
                    {
                        var auditLogInfo = new AuditLogInfo()
                        {
                            ActionTaken = "Authorization Signature Matched Generated Authorization Signature, Indicating that the Event Received from GloCurrency is Valid",
                            Message = $"Header_Authorization_Signature: {header_authorization_signature}, Generated_Authorization_Signature: {gen_auth_signature} Authorization_Nonce: {header_authorization_nonce}",
                            Time = DateTime.Now.ToString()
                        };

                        logger.LogAuditToTextFile(auditLogInfo);

                        // Do something with desrializedResponse
                        if (jsonObj != null)
                        {
                            //log to file
                            var logInfo = new AuditLogInfo()
                            {
                                ActionTaken = "Event from GloCurrency Webhook",
                                Message = $"Type of Event Received: {jsonObj.event_type} Event Response: {gloCurrencyJsonResponse}",
                                Time = DateTime.Now.ToString()
                            };

                            logger.LogAuditToTextFile(logInfo); //

                            //save the collection_pin and processing_item_ID 
                            string transactionId = jsonObj.data.id;
                            string transactionStatus = ConfigHelpers.GloTransactionStatus;
                            string vendor = ConfigHelpers.GloVendor;
                            string currencyCode = jsonObj.data.transaction.output_currency_code;
                            string outputAmount = jsonObj.data.transaction.output_amount;
                            string collectionPin = jsonObj.data.transaction.collection_pin; //collection pin serves as the referenceId
                            string senderFullName = jsonObj.data.transaction.sender.first_name + " " + jsonObj.data.transaction.sender.last_name;
                            string senderDateOfBirth = jsonObj.data.transaction.sender.birth_date;
                            string senderCountryCode = jsonObj.data.transaction.sender.country_code;
                            string recipientFullName = jsonObj.data.transaction.recipient.first_name + " " + jsonObj.data.transaction.recipient.last_name;
                            string recipientEmail = jsonObj.data.transaction.recipient.email;
                            string recipientGender = jsonObj.data.transaction.recipient.gender;
                            string recipientAddress = jsonObj.data.transaction.recipient.street;
                            string recipientDateOfBirth = jsonObj.data.transaction.recipient.birth_date;
                            string recipientPhoneNo = jsonObj.data.transaction.recipient.phone_number;
                            string recipientCountry = jsonObj.data.transaction.recipient.country_code;
                            DateTime createdAt = Convert.ToDateTime(jsonObj.data.transaction.created_at);

                            //log to the database if the processing_item state is pending
                            if (jsonObj.event_type == ConfigHelpers.GloProcessingItemPending)
                            {
                                //sql query
                                var sql = $"INSERT INTO [WorldRemitForCashPickUp].[dbo].[TransactionRequests](TransactionId,ReferenceId,WorldRemitId,TotalAmountReceived,SenderFullName,SenderCountry,SenderDateOfBirth,RecipientFullName,RecipientEmail,RecipientGender,RecipientAddress,RecipientDateOfBirth,TransactionStatus,CreatedOn,RecipientMobilenumber,RecipientCountry,IsDeclined,IsDeleted,CurrencyCode,Vendor,TransactionCreateDate,IsCanceled)" +
                                         $"VALUES('{transactionId}','{collectionPin}','{collectionPin}','{outputAmount}','{senderFullName}','{senderCountryCode}','{senderDateOfBirth}','{recipientFullName}','{recipientEmail}','{recipientGender}','{recipientAddress}','{recipientDateOfBirth}','{transactionStatus}','{createdAt}','{recipientPhoneNo}','{recipientCountry}','{0}','{0}','{currencyCode}','{vendor}','{DateTime.Now}','{0}')";

                                using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["WorldRemmitConnection"].ToString()))
                                {
                                    using (var command = new SqlCommand(sql, connection))
                                    {
                                        connection.Open();
                                        var val1 = command.ExecuteNonQuery();
                                        if (val1 > 0)
                                        {
                                            connection.Close();
                                            var successLogInfo = new AuditLogInfo()
                                            {
                                                ActionTaken = "Save processing_item.pending Records to WorldRemitForCashPickUp Database",
                                                Message = $"Successfully saved the pending processing item into the Database",
                                                Time = DateTime.Now.ToString()
                                            };

                                            logger.LogAuditToTextFile(successLogInfo); //
                                        }
                                        else
                                        {
                                            connection.Close();
                                            var failedLogInfo = new AuditLogInfo()
                                            {
                                                ActionTaken = "Save processing_item.pending Records to WorldRemitForCashPickUp Database",
                                                Message = $"Failed to add/save the pending processing item into the Database",
                                                Time = DateTime.Now.ToString()
                                            };

                                            logger.LogAuditToTextFile(failedLogInfo); //
                                        }
                                    }
                                }
                            }

                            //send mail to Remitance and Business Automation for successful processed items
                            if (jsonObj.event_type == ConfigHelpers.GloProcessingItemProcessed)
                            {
                                string mail = $"{vendor} transaction was processed successfully. See details of the transaction below: </br>" +
                                    $"Sender Full Name: {senderFullName} </br>" +
                                    $"Recipient Full Name: {recipientFullName} </br>" +
                                    $"Transaction ID: {transactionId} </br>" +
                                    $"Collection Pin: {collectionPin}";

                                bool response = MailSender.Sendmail(mail);
                                var infoLog = new AuditLogInfo();

                                if (response == true)
                                {
                                    infoLog.ActionTaken = "Sending Mail to Remittance and Business Automation Team for Successful Processed Item";
                                    infoLog.Message = $"Processing Item With Processing_Item_ID: {transactionId} and Collection Pin: {collectionPin} was Successfully Processed";
                                    infoLog.Time = DateTime.Now.ToString();
                                }
                                else
                                {
                                    infoLog.ActionTaken = "Sending Mail to Remittance and Business Automation Team for Successful Processed Item";
                                    infoLog.Message = $"An Error Occurred While Sending Mail";
                                    infoLog.Time = DateTime.Now.ToString();
                                }

                                logger.LogAuditToTextFile(infoLog); //
                            }
                        }
                        else
                        {
                            var errorLog = new AuditLogInfo()
                            {
                                ActionTaken = "Event from GloCurrency Webhook",
                                Message = $"No Response Event from Glocurrency WebHook",
                                Time = DateTime.Now.ToString()
                            };

                            logger.LogAuditToTextFile(errorLog); //
                        }
                    }
                    else
                    {
                        var errorLogInfo = new AuditLogInfo()
                        {
                            ActionTaken = "Authorization Signature does not Match Generated Authorization Signature, Indicating that the Event Received from GloCurrency is InValid",
                            Message = $"Header_Authorization_Signature: {header_authorization_signature}, Generated_Authorization_Signature: {gen_auth_signature} Authorization_Nonce: {header_authorization_nonce}",
                            Time = DateTime.Now.ToString()
                        };

                        logger.LogAuditToTextFile(errorLogInfo); //
                    }
                }

                return new HttpResponseMessage(HttpStatusCode.OK); //returns 200 
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

                logger.LogErrorToTextFile(errorLogInfo);

                return new HttpResponseMessage(HttpStatusCode.InternalServerError); //returns 500
            }
        }
    }
}
