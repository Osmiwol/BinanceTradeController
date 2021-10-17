using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace TradeController.Sources.Common
{
    class ResponseConverter
    {

        public static string GetResponse(HttpWebRequest request)
        {
            string result = "";

            if (request == null) return result;

            try
            {
                return ReadResponse(request.GetResponse());                
            }
            catch (WebException e)
            {
                return ReadResponse(e.Response);                
            }
        }

        private static string ReadResponse(WebResponse response)
        {
            HttpWebResponse httpResponse = (HttpWebResponse)response;            
            using (Stream data = response.GetResponseStream())
            using (var reader = new StreamReader(data))
            {
                return reader.ReadToEnd();
            }
        }

        public static bool IsResponseError(string response)
        {
            bool result = false;
            
            if (string.IsNullOrEmpty(response)) return result;

            if (response.Contains("code") &&
                response.Contains("msg")) result = true;

            return result;
        }

        public static bool IsResponseBalance(string response)
        {
            bool result = false;

            if (string.IsNullOrEmpty(response)) return result;

            //if (response.Contains("USDT") && response.Contains("BNB") && response.Contains("BUSD")) result = true;
            if (response.Contains("BTCUSDT")) result = true;

            return result;
        }

        /*
         try
            {
                using (WebResponse response = request.GetResponse())
                {
                    Console.WriteLine("Won't get here");
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        Console.WriteLine(text);
                    }
                }
            }
         */

    }
}
