using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using TradeController.Sources.Common;

namespace TradeController.Sources.Services.BinancePerpetualFutureAPI.Order
{
    class Order : IOrder
    {
        string url = "https://fapi.binance.com";
        string parTimeStamp = "timestamp=";
        string parSignature = "signature=";

        int parTimeStampNow;
        HttpWebRequest request;
        HttpWebResponse response;

        HttpClient client;
        HttpResponseMessage resp;
        public string CancelAllOpenOrders(string openKey, string secretKey)
        {
            if (string.IsNullOrEmpty(openKey)) return "";

            client = new HttpClient();


            parTimeStampNow = TimeManager.GetTimeStamp();
            string signature = HmacSHA256.SighText("symbol=BTCUSDT&" + parTimeStamp + parTimeStampNow + "123", secretKey);
            string requestPath = @$"/fapi/v1/allOpenOrders?symbol=BTCUSDT&{parTimeStamp}{parTimeStampNow}123&{parSignature}{signature}";

            //Data( openKey,  secretKey);
            
            request = (HttpWebRequest)WebRequest.Create(url + requestPath);
            request.Method = "DELETE";
            request.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            request.Headers.Add("X-MBX-APIKEY", openKey);            
            request.Headers.Add(HttpRequestHeader.Connection, "keep-alive");
            request.Date = DateTime.Now;
                        
            
            response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            return new StreamReader(stream).ReadToEnd();

        }

        private async void  Data(string openKey, string secretKey)
        {
            parTimeStampNow = TimeManager.GetTimeStamp();
            string signature = HmacSHA256.SighText("symbol=BTCUSDT&" + parTimeStamp + parTimeStampNow + "123", secretKey);
            string requestPath = @$"/fapi/v1/allOpenOrders?symbol=BTCUSDT&{parTimeStamp}{parTimeStampNow}123&{parSignature}{signature}";


            HttpRequestMessage message = new HttpRequestMessage()
            {
                RequestUri = new Uri(url + requestPath),
                Method = HttpMethod.Delete,
            };
            
            message.Headers.Add("ContentType", "application/json");
            message.Headers.Add("X-MBX-APIKEY", openKey);

            var task = await client.SendAsync(message);
            
            Console.WriteLine(resp);

        }
    }
}
