using System;
using System.Collections.Generic;
using System.Text;

namespace TradeController.Sources.Services.BinancePerpetualFutureAPI.Order
{
    interface IOrder
    {
        
        public string CancelAllOpenOrders(string symbol);

        public string CurrentAllOpenOrders(string symbol="");

    }
}
