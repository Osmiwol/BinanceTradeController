using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using TradeController.Sources.Common;

namespace TradeController.Sources.Services.BinancePerpetualFutureAPI.Account
{
    class AccountService : IAccountService
    {
        string url = "https://fapi.binance.com";
        public string GetAccountInformation(string openKey, string closeKey)
        {
            string result = "";
            if (string.IsNullOrEmpty(openKey)) return result;

            int tsNow = TimeManager.GetTimeStamp();
            long timeNow = TimeManager.ConvertToUnixTime(DateTime.Now);

            string timeParameter = $"timestamp={tsNow}333";

            //string timeParameter = $"timestamp=1632305994009";
            string subscribe = HmacSHA256.SighText(timeParameter, closeKey);

            string signature = $"signature={subscribe}";
            string CheckServerTime = @$"/fapi/v2/account?{timeParameter}&{signature}";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + CheckServerTime);
            request.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            request.Headers.Add("X-MBX-APIKEY", openKey);
            request.Headers.Add(HttpRequestHeader.Accept, "*/*");
            request.Headers.Add(HttpRequestHeader.Connection, "keep-alive");
            request.Date = DateTime.Now;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            result = sr.ReadToEnd();

            return result;
        }

        public Stream GetAccountInformationStream(string Api)
        {
            throw new NotImplementedException();
        }
    }
}
