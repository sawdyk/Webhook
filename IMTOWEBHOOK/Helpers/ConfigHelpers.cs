using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace IMTOWEBHOOK.Helpers
{
    public static class ConfigHelpers
    {
        public static string GloTransactionStatus
        {
            get
            {
                return ConfigurationManager.AppSettings["GloTransactionStatus"].ToString();
            }
        }

        public static string GloVendor
        {
            get
            {
                return ConfigurationManager.AppSettings["GloVendor"].ToString();
            }
        }

        public static string GloHttpVerb
        {
            get
            {
                return ConfigurationManager.AppSettings["GloHttpVerb"].ToString();
            }
        }

        public static string GloProcessingItemPending
        {
            get
            {
                return ConfigurationManager.AppSettings["GloProcessingItemPending"].ToString();
            }
        }

        public static string GloProcessingItemProcessed
        {
            get
            {
                return ConfigurationManager.AppSettings["GloProcessingItemProcessed"].ToString();
            }
        }
    }
}