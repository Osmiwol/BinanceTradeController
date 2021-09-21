using System;
using System.Collections.Generic;
using System.Text;

namespace TradeController.Sources.Services.BinancePerpetualFutureAPI.Order
{
    interface IOrder
    {
        public bool CancelAllOpenOrders();
    }
}
