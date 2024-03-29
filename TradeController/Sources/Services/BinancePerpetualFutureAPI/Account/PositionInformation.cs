﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using TradeController.Sources.Common;

namespace TradeController.Sources.Services.BinancePerpetualFutureAPI.Account
{
    class PositionInformation
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
            LoggerWriter.LogAndConsole("инициализированы поля PositionInformation.SetParameters");
            this.url = url;
            this.openKey = openKey;
            this.closeKey = closeKey;
        }
        public string GetPostitionInformation()
        {
            LoggerWriter.LogAndConsole("Вызван метод GetPostitionInformation\n");
            parTimeStampNow = TimeManager.GetTimeStamp();
            string signature = HmacSHA256.SighText(parTimeStamp + parTimeStampNow + "123", closeKey);
            string parGetAccountPath = @$"/fapi/v2/positionRisk?{parTimeStamp}{parTimeStampNow}123&{parSignature}{signature}";

            requestGetAccountData = (HttpWebRequest)WebRequest.Create(url + parGetAccountPath);

            requestGetAccountData.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            requestGetAccountData.Headers.Add("X-MBX-APIKEY", openKey);

            return ResponseConverter.GetResponse(requestGetAccountData);
        }

    }
}
