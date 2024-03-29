﻿using System;
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
            LoggerWriter.LogAndConsole("GetResponse");
            string result = "";

            if (request == null) return result;

            try
            {
                LoggerWriter.LogAndConsole("try ReadResponse");
                return ReadResponse(request.GetResponse());
            }
            catch (WebException e)
            {
                LoggerWriter.LogAndConsole("GetResponseException");
                return ReadResponse(e.Response);
            }
        }
        private static string ReadResponse(WebResponse response)
        {
            LoggerWriter.LogAndConsole("Вызыван ReadResponse");

            

            HttpWebResponse httpResponse = (HttpWebResponse)response;
            using (Stream data = response.GetResponseStream())
            using (var reader = new StreamReader(data))
            {
                return reader.ReadToEnd();
            }       
            
        }

        public static bool IsResponseError(string response)
        {
            LoggerWriter.LogAndConsole("Вызыван IsResponseError");
            bool result = false;
            
            if (string.IsNullOrEmpty(response)) return result;

            if (response.Contains("code") &&
                response.Contains("msg")) result = true;

            LoggerWriter.LogAndConsole($"IsResponseError:{result}");

            return result;
        }

        public static bool IsResponseBalance(string response)
        {
            LoggerWriter.LogAndConsole("Вызыван IsResponseBalance");
            bool result = false;

            if (string.IsNullOrEmpty(response)) return result;

            
            if (response.Contains("BTCUSDT")) result = true;
            LoggerWriter.LogAndConsole($"IsResponseBalance:{result}");
            return result;
        }


    }
}
