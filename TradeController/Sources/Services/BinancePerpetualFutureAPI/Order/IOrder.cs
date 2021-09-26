using System;
using System.Collections.Generic;
using System.Text;

namespace TradeController.Sources.Services.BinancePerpetualFutureAPI.Order
{
    interface IOrder
    {
        public string CancelAllOpenOrders(string openKey, string secretKey);
    }
}
