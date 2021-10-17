using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using TradeController.Sources.Common;
using TradeController.Sources.Model;

namespace TradeController.Sources.Services.BinancePerpetualFutureAPI.Order
{
    class CloseAllPositions
    {
        string path = "";
        string local = "/fapi/v1/order?";
        
        const string parSignature = "signature=";
        string type = "";

        int parTimeStampNow;
        HttpWebRequest requestGetAccountData;

        string url;
        string openKey;
        string closeKey;

        public void SetParameters(string url, string openKey, string closeKey)
        {
            this.url = url;
            this.openKey = openKey;
            this.closeKey = closeKey;
        }


        public string CloseShort(Position position)
        {
            type = "BUY";

            if (position == null) return "{code:-1, msg: short position was empty }";

            double quantity =Math.Abs(position.positionAmt) + Math.Abs(position.notional);
            quantity = Math.Round(quantity, 0);

            path = $"symbol={position.symbol}&side={type}&type=MARKET&quantity={quantity}&reduceOnly=true&newOrderRespType=FULL&timestamp=";
            //path = $"symbol={position.symbol}&side={type}&type=MARKET&quantity={1}&reduceOnly=true&newOrderRespType=FULL&timestamp=";

            parTimeStampNow = TimeManager.GetTimeStamp();

            string signature = HmacSHA256.SighText(path + parTimeStampNow + "123", closeKey);
            string parGetAccountPath = $"{local}{path}{parTimeStampNow}123&{parSignature}{signature}";
            requestGetAccountData = Common.CreateRequest("POST", url, parGetAccountPath, openKey);
            string res = ResponseConverter.GetResponse(requestGetAccountData);
            Console.WriteLine(res);
            return res;

        }

        public string CloseLong(Position position)
        {
            type = "SELL";

            if (position == null) return "{code:-1, msg: long position was empty }";

            double quantity = Math.Abs(position.positionAmt) + Math.Abs(position.notional);
            quantity = Math.Round(quantity, 0);

            path = $"symbol={position.symbol}&side={type}&type=MARKET&quantity={quantity}&reduceOnly=true&newOrderRespType=FULL&timestamp=";
            //path = $"symbol={position.symbol}&side={type}&type=MARKET&quantity={1}&reduceOnly=true&newOrderRespType=FULL&timestamp=";

            parTimeStampNow = TimeManager.GetTimeStamp();

            string signature = HmacSHA256.SighText(path + parTimeStampNow + "123", closeKey);
            string parGetAccountPath = $"{local}{path}{parTimeStampNow}123&{parSignature}{signature}";
            requestGetAccountData = Common.CreateRequest("POST", url, parGetAccountPath, openKey);
            string res = ResponseConverter.GetResponse(requestGetAccountData);
            Console.WriteLine(res);
            return res;

        }


       

    }
}
