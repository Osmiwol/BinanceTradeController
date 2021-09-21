using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TradeController.Sources.Services.BinancePerpetualFutureAPI
{
    interface IMarket
    {
        public string TestConnectivity();
        public string CheckServerTime();

        public Stream TestConnectivityStream();
        public Stream CheckServerTimeStream();
    }
}
