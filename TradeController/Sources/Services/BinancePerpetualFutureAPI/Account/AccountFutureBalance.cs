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
        HttpWebResponse responseAccountData;
        public  string GetAccountBalances(string url,string openKey, string closeKey)
        {
            string result = "";
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

            try
            {
                responseAccountData = (HttpWebResponse)requestGetAccountData.GetResponse();
                Stream stream = responseAccountData.GetResponseStream();
                result = new StreamReader(stream).ReadToEnd();
            }
            catch(Exception ex)
            {
                File.AppendAllText("BalanceLog.txt", $"\n{DateTime.Now} Произошла ошибка при попытке считывания баланса: {ex}\n");
            }

            return result;
        }
    }
}
