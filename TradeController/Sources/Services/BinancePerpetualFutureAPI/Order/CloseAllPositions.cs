﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using TradeController.Sources.Common;

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
        HttpWebResponse responseAccountData;

        string url;
        string openKey;
        string closeKey;

        public void SetParameters(string url, string openKey, string closeKey)
        {
            this.url = url;
            this.openKey = openKey;
            this.closeKey = closeKey;
        }
        public string CloseShort()
        {

            type = "BUY";

            path = $"symbol=BTCUSDT&side={type}&type=MARKET&quantity=1&reduceOnly=true&newOrderRespType=FULL&timestamp=";

            parTimeStampNow = TimeManager.GetTimeStamp();
            
            string signature = HmacSHA256.SighText(path + parTimeStampNow + "123", closeKey);                        
            string parGetAccountPath = $"{local}{path}{parTimeStampNow}123&{parSignature}{signature}";
            requestGetAccountData = CreateRequest("POST", url, parGetAccountPath, openKey);
            return ResponseConverter.GetResponse(requestGetAccountData);
            /*
            try
            {
                responseAccountData = (HttpWebResponse)requestGetAccountData.GetResponse();
                Stream stream = responseAccountData.GetResponseStream();
                result = new StreamReader(stream).ReadToEnd(); 
            }
            catch(Exception ex)
            {
                File.AppendAllText("_LOG_LogClosingPositions.txt", $"\n{DateTime.Now} Произошла ошибка при попытке закрыть Short, но скорее всего все окей! " + ex);
            }
            */

            
        }
        public string CloseLong()
        {

            type = "SELL";
            path = $"symbol=BTCUSDT&side={type}&type=MARKET&quantity=1&reduceOnly=true&newOrderRespType=FULL&timestamp=";

            parTimeStampNow = TimeManager.GetTimeStamp();

            string signature = HmacSHA256.SighText(path + parTimeStampNow + "123", closeKey);
            string parGetAccountPath = $"{local}{path}{parTimeStampNow}123&{parSignature}{signature}";
            requestGetAccountData = CreateRequest("POST",url, parGetAccountPath, openKey);

            return ResponseConverter.GetResponse(requestGetAccountData);

            /*
            try
            { 
                responseAccountData = (HttpWebResponse)requestGetAccountData.GetResponse();
                Stream stream = responseAccountData.GetResponseStream();
                result = new StreamReader(stream).ReadToEnd();
            }
            catch(Exception ex)
            {                
                File.AppendAllText("_LOG_LogClosingPositions.txt", $"\n{DateTime.Now} Произошла ошибка при попытке закрыть Long, но скорее всего все окей! " + ex);
            }
            */

            
        }

        public string CloseOpenPositions()
        {
              
            path = $"/fapi/v1/allOpenOrders?symbol=BTCUSDT&timestamp=";

            parTimeStampNow = TimeManager.GetTimeStamp();

            string signature = HmacSHA256.SighText("symbol=BTCUSDT&timestamp=" + parTimeStampNow + "123", closeKey);
            string parGetAccountPath = $"{path}{parTimeStampNow}123&{parSignature}{signature}";
            requestGetAccountData = CreateRequest("DELETE", url, parGetAccountPath, openKey);
            return ResponseConverter.GetResponse(requestGetAccountData);
            /*
            try
            {
                responseAccountData = (HttpWebResponse)requestGetAccountData.GetResponse();
                Stream stream = responseAccountData.GetResponseStream();
                result = new StreamReader(stream).ReadToEnd();
            }
            catch (Exception ex)
            {
                File.AppendAllText("_LOG_LogClosingPositions.txt", $"\n{DateTime.Now} Произошла ошибка при попытке закрыть открытые позиции, но скорее всего все окей! " + ex);
            }
            */
            
        }



        private HttpWebRequest CreateRequest(string typeRequest,string url,string path, string openKey)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + path);
            request.Method = typeRequest;
            request.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            request.Headers.Add("X-MBX-APIKEY", openKey);
            request.Headers.Add(HttpRequestHeader.Accept, "*/*");
            request.Headers.Add(HttpRequestHeader.Connection, "keep-alive");
            request.Date = DateTime.Now;

            return request;

        }

    }
}