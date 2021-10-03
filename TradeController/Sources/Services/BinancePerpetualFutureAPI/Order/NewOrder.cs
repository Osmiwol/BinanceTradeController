using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using TradeController.Sources.Common;

namespace TradeController.Sources.Services.BinancePerpetualFutureAPI.Order
{
    class NewOrder : INewOrder
    {

        //string url = "https://fapi.binance.com";
        const string URL = "https://testnet.binancefuture.com/fapi/v1/order?";

        int parTimeStampNow;

        string parSymbol = "symbol=BTCUSDT";
        string parSide = "side=";
        string parType = "type=MARKET";
        string parQuantitiy = "quantity=1";
        string parReduseOnly = "reduceOnly=true";
        string parNewOrderRespType = "newOrderRespType=FULL";

        string parTimeStamp = "timestamp=";
        string parSignature = "signature=";
        string parSignatureValue = "";

        HttpWebRequest request;
        HttpWebResponse response;

        HttpClient client;
        HttpResponseMessage resp;

        public string CloseLongPosition(string openKey, string secretKey)
        {
            if (string.IsNullOrEmpty(openKey)) return "";

            client = new HttpClient();
            string requestPath = RequestPathClosePositon(true, secretKey);
            request = CreateRequest(requestPath, openKey);
            
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception is: " + ex);
            }

            Stream stream = response.GetResponseStream();
            
            return new StreamReader(stream).ReadToEnd();
        }

        public string CloseShortPosition(string openKey, string secretKey)
        {
            if (string.IsNullOrEmpty(openKey)) return "";

            client = new HttpClient();
            string requestPath = RequestPathClosePositon(false, secretKey);
            request = CreateRequest(requestPath, openKey);
            response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();

            return new StreamReader(stream).ReadToEnd();
        }

        /// <summary>
        /// Make path to close position (CloseLong = true)
        /// </summary>
        /// <param name="postitionType">true = posSide = Sell</param>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        private string RequestPathClosePositon(bool postitionType,string secretKey)
        {
            string result = "";
            string posSide = "";

            if (postitionType) posSide = "SELL";
            else posSide = "BUY";

            parTimeStampNow = TimeManager.GetTimeStamp();
            //result = $@"/fapi/v1/order?{parSymbol}&{parSide}{posSide}&{parType}&{parQuantitiy}&";

            result = $@"{parSymbol}&{parSide}{posSide}&{parType}&{parQuantitiy}&";
            result += $@"{parReduseOnly}&{parNewOrderRespType}&{parTimeStamp}{parTimeStampNow}000&";
            //result += $@"{parReduseOnly}&{parNewOrderRespType}&{parTimeStamp}1633264088429";

            parSignatureValue = HmacSHA256.SighText(result, secretKey);
            //parSignatureValue = "ass";

            result += $@"{parSignature}{parSignatureValue}";
            string comm = URL + result;
            return comm;
        }

        private HttpWebRequest CreateRequest(string requestPath, string openKey)
        {
            HttpWebRequest result = (HttpWebRequest)WebRequest.Create(requestPath);
            result = (HttpWebRequest)WebRequest.Create(requestPath);
            result.Method = "POST";
            result.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            result.Headers.Add("X-MBX-APIKEY", openKey);
            result.Headers.Add(HttpRequestHeader.Connection, "keep-alive");
            result.Date = DateTime.Now;

            return result;
        }

    }
}
