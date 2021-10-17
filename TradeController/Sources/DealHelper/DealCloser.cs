using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
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
            string result = "";

            if (positions == null || positions.Count < 1) return result;

            for (int i = 0; i < positions.Count; i++)
            {
                if (positions[i].notional > 0)
                    result += closeAllPositions.CloseLong(positions[i]);
                else 
                    if (positions[i].notional < 0)
                        result += closeAllPositions.CloseShort(positions[i]);
            }

            return result;
        }


    }
}
