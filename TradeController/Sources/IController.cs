using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace TradeController.Sources
{
    interface IController
    {
        public void AddDataShower(Action<string> action);
        public void AddDataCleaner(Action action);
        public void StartMonitoring( int lowBorder);

        public void CancelAllOpenOrders();
    }
}
