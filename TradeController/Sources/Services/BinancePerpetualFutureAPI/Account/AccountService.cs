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
        string parTimeStamp = "timestamp=";
        string parSignature = "signature=";
        
        int parTimeStampNow;
        HttpWebRequest requestGetAccountData;
        HttpWebResponse responseAccountData;
        public string GetAccountInformation(string openKey, string closeKey)
        {            
            if (string.IsNullOrEmpty(openKey)) return "";
            
            parTimeStampNow = TimeManager.GetTimeStamp();
            string signature = HmacSHA256.SighText(parTimeStamp + parTimeStampNow + "123", closeKey);
            string parGetAccountPath = @$"/fapi/v2/account?{parTimeStamp}{parTimeStampNow}123&{parSignature}{signature}";

            requestGetAccountData = (HttpWebRequest)WebRequest.Create(url + parGetAccountPath);
            requestGetAccountData.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            requestGetAccountData.Headers.Add("X-MBX-APIKEY", openKey);
            requestGetAccountData.Headers.Add(HttpRequestHeader.Accept, "*/*");
            requestGetAccountData.Headers.Add(HttpRequestHeader.Connection, "keep-alive");
            requestGetAccountData.Date = DateTime.Now;

            responseAccountData = (HttpWebResponse)requestGetAccountData.GetResponse();
            Stream stream = responseAccountData.GetResponseStream();

            return new StreamReader(stream).ReadToEnd();
        }

        public Stream GetAccountInformationStream(string Api)
        {
            throw new NotImplementedException();
        }
    }
}
