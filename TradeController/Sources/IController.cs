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

        public void AddBalanceShow(Action<float> action);
        public void AddAvailableShow(Action<float> action);
        public void AddIterMonitoring(Action<int> iter);

        public void StartMonitoring(CancellationTokenSource cts,int lowBorder );

        public void CleanAllEvents();

        public void CancelAllOpenOrders();
    }
}
