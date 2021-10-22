using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using TradeController.Sources.Common;
using TradeController.Sources.Model;
using TradeController.Sources.Services.BinancePerpetualFutureAPI.Account;
using TradeController.Sources.Services.BinancePerpetualFutureAPI.Order;

namespace TradeController.Sources.DealHelper
{
    class DealCloser
    {
        string[] keys;
        string url;

        static IOrder order;
        PositionInformation positionInfo;
        CloseAllPositions closeAllPositions;
        public DealCloser(string url,string[] keys)
        {
            this.keys = keys;
            this.url = url;

            order = new Order(url, keys[0], keys[1]);
            positionInfo = new PositionInformation();
            positionInfo.SetParameters(url, keys[0], keys[1]);

            closeAllPositions = new CloseAllPositions();
            closeAllPositions.SetParameters(url, keys[0], keys[1]);
        }

        public  string CloseOpenDeals()
        {
            string result = "";
            
            List<OpenPosition> openPositions = new List<OpenPosition>();

            string orders = order.CurrentAllOpenOrders();
            
            if (string.IsNullOrEmpty(orders) || orders == "[]") return result;

            if (orders.Contains("code") && orders.Contains("msg"))
            {
                Console.WriteLine(orders);
                return orders;
            }
            else
                openPositions = JsonConvert.DeserializeObject<List<OpenPosition>>(orders);

            for (int i = 0; i < openPositions.Count; i++)
            {
                string response = order.CancelAllOpenOrders(openPositions[i].symbol);

                if (response.Contains("code") && response.Contains("msg") && response.Contains("200"))
                {
                    Console.WriteLine(response);
                    result += openPositions[i].symbol + "- Done!";
                }
                else
                {
                    result += response;
                    return result;
                }
            }

            return result;
        }
        
        public string CloseDeals(List<Position> positions)
        {
            LoggerWriter.LogAndConsole($"Вызван метод CloseDeals! Количество позиций: {positions.Count}\n");
            string result = "";

            if (positions == null || positions.Count < 1) return result;

            for (int i = 0; i < positions.Count; i++)
            {
                if (positions[i].notional > 0)
                {
                    LoggerWriter.LogAndConsole($"Закрытие позиции лонг!\n");
                    result += closeAllPositions.CloseLong(positions[i], true);
                    result += closeAllPositions.CloseLong(positions[i], false);
                }
                else
                    if (positions[i].notional < 0)
                    {
                        LoggerWriter.LogAndConsole($"Закрытие позиции шорт!\n");
                        result += closeAllPositions.CloseShort(positions[i],true);
                        result += closeAllPositions.CloseShort(positions[i],false);
                    }
            }

            LoggerWriter.LogAndConsole($"Функция closeDeals завершена!\n");
            return result;
        }


    }
}
