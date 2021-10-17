using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using TradeController.Sources.Common;

namespace TradeController.Sources.Services.BinancePerpetualFutureAPI.Order
{
    class CancelAllOpenOrders
    {
        string url = "";
        string local = "/fapi/v1/openOrders?";
        
        string parTimeStamp = "timestamp=";
        string parSignature = "signature=";
        string parSymbol = "symbol=";


        int parTimeStampNow;
        HttpWebRequest requestGetAccountData;

        private string openKey;
        private string closeKey;
        public void SetParameters(string url, string openKey, string closeKey)
        {
            this.url = url;
            this.openKey = openKey;
            this.closeKey = closeKey;
        }
        public string Cancel()
        {
            parTimeStampNow = TimeManager.GetTimeStamp();
            string signature = HmacSHA256.SighText(parTimeStamp + parTimeStampNow + "123", closeKey);
            string parGetAccountPath = @$"{local}{parTimeStamp}{parTimeStampNow}123&{parSignature}{signature}";

            requestGetAccountData = (HttpWebRequest)WebRequest.Create(url + parGetAccountPath);

            requestGetAccountData.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            requestGetAccountData.Headers.Add("X-MBX-APIKEY", openKey);



            return ResponseConverter.GetResponse(requestGetAccountData);
        }
    }
}
