using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using TradeController.Sources.Common;

namespace TradeController.Sources.Services.BinancePerpetualFutureAPI.Account
{
    class AccountInformation
    {

        string url = "";
        string parTimeStamp = "timestamp=";
        string parSignature = "signature=";

        int parTimeStampNow;
        HttpWebRequest requestGetAccountData;
        WebResponse response;

        private string openKey;
        private string closeKey;
        public void SetParameters(string url, string openKey, string closeKey)
        {
            this.url = url;
            this.openKey = openKey;
            this.closeKey = closeKey;
        }
        public string GetAccountBalances()
        {
            parTimeStampNow = TimeManager.GetTimeStamp();
            string signature = HmacSHA256.SighText(parTimeStamp + parTimeStampNow + "123", closeKey);
            string parGetAccountPath = @$"/fapi/v2/account?{parTimeStamp}{parTimeStampNow}123&{parSignature}{signature}";

            requestGetAccountData = (HttpWebRequest)WebRequest.Create(url + parGetAccountPath);

            requestGetAccountData.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            requestGetAccountData.Headers.Add("X-MBX-APIKEY", openKey);

            /*
            response = requestGetAccountData.GetResponse();
            var reader =  new StreamReader(response.GetResponseStream());
            return reader.ReadToEnd();            
            */

            return ResponseConverter.GetResponse(requestGetAccountData);
        }

    }
}
