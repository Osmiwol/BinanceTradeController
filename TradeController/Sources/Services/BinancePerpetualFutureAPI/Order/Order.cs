using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using TradeController.Sources.Common;

namespace TradeController.Sources.Services.BinancePerpetualFutureAPI.Order
{
    class Order : IOrder
    {
        string url;
        string openKey;
        string closeKey;

        int timeStamp;
        HttpWebRequest request;
        WebResponse response;


        string parSymbol = "symbol=";
        string parTimeStamp = "timestamp=";
        string parSignature = "signature=";
        public Order(string url, string openKey,string closeKey)
        {
            LoggerWriter.LogAndConsole("создан экземпляр класса Order");
            this.url = url;
            this.openKey = openKey;
            this.closeKey = closeKey;
        }

        public string CancelAllOpenOrders(string symbol)
        {
            LoggerWriter.LogAndConsole("CancelAllOpenOrders");
            string local = "/fapi/v1/allOpenOrders?";

            timeStamp = TimeManager.GetTimeStamp();
            string parametersForSign = $"{parSymbol}{symbol}&{parTimeStamp}{timeStamp}123";
            string signature = HmacSHA256.SighText(parametersForSign, closeKey);
            string fullParameters = $"{parametersForSign}&{parSignature}{signature}";
            string fullPath = local + fullParameters;
            Console.WriteLine($"\n{local}\n{fullParameters}\n{fullPath}");

            request = Common.CreateRequest("DELETE", url, fullPath, openKey);

            LoggerWriter.LogAndConsole("CancelAllOpenOrders завершен");
            return ResponseConverter.GetResponse(request);
        }

        public string CurrentAllOpenOrders(string symbol="")
        {
            LoggerWriter.LogAndConsole("CurrentAllOpenOrders");
            string local = "/fapi/v1/openOrders?";

            timeStamp = TimeManager.GetTimeStamp();
            string parametersForSign;
            
            if (string.IsNullOrEmpty(symbol))
                parametersForSign = $"{parTimeStamp}{timeStamp}123";
            else
                parametersForSign = $"{parSymbol}{symbol}&{parTimeStamp}{timeStamp}123";

            string signature = HmacSHA256.SighText(parametersForSign, closeKey);
            string fullParameters = $"{parametersForSign}&{parSignature}{signature}";
            string fullPath = local + fullParameters;

            request = Common.CreateRequest("GET", url, fullPath, openKey);
            
            LoggerWriter.LogAndConsole("CurrentAllOpenOrders завершен");
            return ResponseConverter.GetResponse(request);
        }
        

    }
}
