using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IMTOWEBHOOK.Models
{
    public class GloCurrencyModels
    {
        public class Webhook
        {
            public string event_type { get; set; }
            public string uri { get; set; }
        }

        public class WebhookData
        {
            public string webhook_id { get; set; }
            public string event_type { get; set; }
            public data data { get; set; }
        }

        public class data
        {
            public string id { get; set; }
            public string state { get; set; }
            public corridor corridor { get; set; }
            public transaction transaction { get; set; }
        }

        public class corridor
        {
            public bank bank { get; set; }
            public string country_code { get; set; }
            public string currency_code { get; set; }
            public string transaction_type { get; set; }
        }

        public class bank
        {
            public string code { get; set; }
            public string name { get; set; }
            public string country_code { get; set; }
        }

        public class sender
        {
            public string type { get; set; }
            public string state { get; set; }
            public string last_name { get; set; }
            public string birth_date { get; set; }
            public string first_name { get; set; }
            public string country_code { get; set; }
        }
        public class recipient
        {
            public string city { get; set; }
            public string email { get; set; }
            public string state { get; set; }
            public string gender { get; set; }
            public string street { get; set; }
            public string last_name { get; set; }
            public string birth_date { get; set; }
            public string first_name { get; set; }
            public string postal_code { get; set; }
            public string country_code { get; set; }
            public string phone_number { get; set; }
        }

        public class transaction
        {
            public string id { get; set; }
            public string type { get; set; }
            public string state { get; set; }
            public string output_amount { get; set; }
            public string collection_pin { get; set; }
            public string output_currency_code { get; set; }
            public string output_amount_in_cents { get; set; }
            public sender sender { get; set; }
            public recipient recipient { get; set; }
            public string created_at { get; set; }
        }

        public class otp
        {
            public string otp_code { get; set; }
        }
    }
}