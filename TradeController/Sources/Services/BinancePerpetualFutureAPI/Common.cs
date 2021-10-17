using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace TradeController.Sources.Services.BinancePerpetualFutureAPI
{
    public class Common
    {
        public static HttpWebRequest CreateRequest(string typeRequest, string url, string path, string openKey)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + path);
            request.Method = typeRequest;
            request.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            request.Headers.Add("X-MBX-APIKEY", openKey);
            request.Headers.Add(HttpRequestHeader.Accept, "*/*");
            request.Headers.Add(HttpRequestHeader.Connection, "keep-alive");
            request.Date = DateTime.Now;

            return request;
        }



    }
}
