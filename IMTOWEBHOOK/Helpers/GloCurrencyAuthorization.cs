using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace IMTOWEBHOOK.Helpers
{
    public class GloCurrencyAuthorization
    {
        public static string authorization_signature(string authorizationNonce, string httpVerb, string requestUrl, string encryptedReqBody)
        {
            string string_to_sign = authorizationNonce + "&" + httpVerb.ToUpper() + "&" + requestUrl + "&" + encryptedReqBody;

            string key = ConfigurationManager.AppSettings["GloCurrencyApiSecret"].ToString();

            byte[] text_bytes = Encoding.ASCII.GetBytes(string_to_sign);
            byte[] key_bytes = Encoding.ASCII.GetBytes(key);
            HMACSHA512 sha = new HMACSHA512(key_bytes);
            byte[] sha_bytes = sha.ComputeHash(text_bytes);
            string sha_text = System.BitConverter.ToString(sha_bytes);
            sha_text = sha_text.Replace("-", "");
            string authorization_signature = sha_text.ToLower();

            return authorization_signature;

        }
    }
}