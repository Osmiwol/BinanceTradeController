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

        bool quantOneShort = true;
        bool quantOneLong = true;

        public void SetParameters(string url, string openKey, string closeKey)
        {
            LoggerWriter.LogAndConsole($"Установка параметров класса CloseAllPositions. Вызыван метод SetParameters\n");
            this.url = url;
            this.openKey = openKey;
            this.closeKey = closeKey;
        }


        public string CloseShort(Position position,bool typeQuantity)
        {
            LoggerWriter.LogAndConsole($"Вызывана функция CloseShort\n");
            type = "BUY";

            if (position == null) return "{code:-1, msg: short position was empty }";
            
            if (typeQuantity)
            {
                LoggerWriter.LogAndConsole($"Вызывана функция CloseShort с quantity = 1\n");
                path = $"symbol={position.symbol}&side={type}&type=MARKET&quantity={1}&reduceOnly=true&newOrderRespType=FULL&timestamp=";
                quantOneShort = false;
                
            }
            else
            {
                double quantity = Math.Abs(position.positionAmt) + Math.Abs(position.notional);
                quantity = Math.Round(quantity, 0);
                path = $"symbol={position.symbol}&side={type}&type=MARKET&quantity={quantity}&reduceOnly=true&newOrderRespType=FULL&timestamp=";
                LoggerWriter.LogAndConsole($"Вызывана функция CloseShort с quantity = {quantity}\n");
            }
            //path = $"symbol={position.symbol}&side={type}&type=MARKET&quantity={1}&reduceOnly=true&newOrderRespType=FULL&timestamp=";
            LoggerWriter.LogAndConsole($"Формирование запроса на закрытие шорт позиции\n");
            parTimeStampNow = TimeManager.GetTimeStamp();

            string signature = HmacSHA256.SighText(path + parTimeStampNow + "123", closeKey);
            string parGetAccountPath = $"{local}{path}{parTimeStampNow}123&{parSignature}{signature}";
            requestGetAccountData = Common.CreateRequest("POST", url, parGetAccountPath, openKey);
            string res = ResponseConverter.GetResponse(requestGetAccountData);
            LoggerWriter.LogAndConsole($"Получен результат после запроса на закрытие шорт позиции!\n");
            LoggerWriter.LogAndConsole(res);


            return res;

        }

        public string CloseLong(Position position,bool typeQuantity)
        {
            type = "SELL";
            LoggerWriter.LogAndConsole($"Вызвана функция CloseLong!\n");
            if (position == null) return "{code:-1, msg: long position was empty }";

            if (typeQuantity)
            {
                path = $"symbol={position.symbol}&side={type}&type=MARKET&quantity={1}&reduceOnly=true&newOrderRespType=FULL&timestamp=";
                quantOneLong = false;
                LoggerWriter.LogAndConsole($"Вызывана функция CloseShort с quantity = 1\n");
                //CloseLong(position);
            }
            else
            {
                double quantity = Math.Abs(position.positionAmt) + Math.Abs(position.notional);
                quantity = Math.Round(quantity, 0);

                path = $"symbol={position.symbol}&side={type}&type=MARKET&quantity={quantity}&reduceOnly=true&newOrderRespType=FULL&timestamp=";
                LoggerWriter.LogAndConsole($"Вызывана функция CloseShort с quantity = {quantity}\n");
            }
            //path = $"symbol={position.symbol}&side={type}&type=MARKET&quantity={1}&reduceOnly=true&newOrderRespType=FULL&timestamp=";

            parTimeStampNow = TimeManager.GetTimeStamp();
            LoggerWriter.LogAndConsole($"Формирование запроса на закрытие лонг позиции\n");
            string signature = HmacSHA256.SighText(path + parTimeStampNow + "123", closeKey);
            string parGetAccountPath = $"{local}{path}{parTimeStampNow}123&{parSignature}{signature}";
            requestGetAccountData = Common.CreateRequest("POST", url, parGetAccountPath, openKey);
            string res = ResponseConverter.GetResponse(requestGetAccountData);
            LoggerWriter.LogAndConsole($"Получен результат после запроса на закрытие лонг позиции!\n");
            LoggerWriter.LogAndConsole(res);

            return res;

        }


       

    }
}
