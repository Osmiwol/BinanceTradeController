using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using TradeController.Sources.Common;

namespace TradeController.Sources.Services.BinancePerpetualFutureAPI.Account
{
    class AccountFutureBalance
    {
        string url = "";
        string parTimeStamp = "timestamp=";
        string parSignature = "signature=";

        int parTimeStampNow;
        HttpWebRequest requestGetAccountData;        
        public  string GetAccountBalances(string url,string openKey, string closeKey)
        {            
            if (string.IsNullOrEmpty(openKey) || string.IsNullOrEmpty(url)) return "";
            this.url = url;

            parTimeStampNow = TimeManager.GetTimeStamp();
            string signature = HmacSHA256.SighText(parTimeStamp + parTimeStampNow + "123", closeKey);
            string parGetAccountPath = @$"/fapi/v2/balance?{parTimeStamp}{parTimeStampNow}123&{parSignature}{signature}";

            requestGetAccountData = (HttpWebRequest)WebRequest.Create(url + parGetAccountPath);
            requestGetAccountData.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            requestGetAccountData.Headers.Add("X-MBX-APIKEY", openKey);
            requestGetAccountData.Headers.Add(HttpRequestHeader.Accept, "*/*");
            requestGetAccountData.Headers.Add(HttpRequestHeader.Connection, "keep-alive");
            requestGetAccountData.Date = DateTime.Now;
            
            return ResponseConverter.GetResponse(requestGetAccountData);
        }
    }
}
