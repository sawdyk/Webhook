using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace IMTOWEBHOOK.RequestModels
{
    public class MailRequestModel
    {
        public string sender { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public string cc { get; set; }
        public string bcc { get; set; }
        public string subject { get; set; }
        public string body { get; set; }
        public List<Attachment> attachments { get; set; }
    }
}