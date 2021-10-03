using System;
using System.Collections.Generic;
using System.Text;

namespace TradeController.Sources.Services.BinancePerpetualFutureAPI.Order
{
    interface INewOrder
    {
        public string CloseLongPosition(string openKey, string secretKey);
        public string CloseShortPosition(string openKey, string secretKey);
    }
}
