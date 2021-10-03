using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using TradeController.Sources.Common;
using TradeController.Sources.Model;
using TradeController.Sources.Services.BinancePerpetualFutureAPI;
using TradeController.Sources.Services.BinancePerpetualFutureAPI.Account;
using TradeController.Sources.Services.BinancePerpetualFutureAPI.Order;

namespace TradeController.Sources
{
    class Controller : IController
    {
        Action<string> _dataFieldShow;
        Action _dataFieldClean;
        Action<float> _balanceShow;
        Action<float> _availableBalanceShow;
        Action<int> _iterMonitoring;
        CancellationToken _ct;
        IAccountService accountService;
        int _lowBorder = 0;
        string[] keys;

        IOrder order;

        //костыльно, но работает
        CloseAllPositions closer;
        AccountFutureBalance balanceFutures;
        //string url = "https://testnet.binancefuture.com"; //тестовый url
        string url = "https://fapi.binance.com";
        //
        public Controller(CancellationTokenSource cts, string pathToKeys)
        {


            if (cts == null)
            {
                _dataFieldShow?.Invoke("Ошибка! Токен не был передан!" + _restartAndMessage);
                return;
            }
            _ct = cts.Token;

            if (string.IsNullOrEmpty(pathToKeys))
            {
                _dataFieldShow?.Invoke("Ошибка! Путь к файлу с ключами не был указан!" + _restartAndMessage);
                return;
            }
            if (!File.Exists(pathToKeys))
            {
                _dataFieldShow?.Invoke("Ошибка! Файл не существует, по указанному пути!" + _restartAndMessage);
                return;
            }
            keys = KeysParser.GetKeys(FileManager.GetTextFromFile(pathToKeys));
            if (keys.Length < 2)
            {
                _dataFieldClean();
                _dataFieldShow("Ошибка! В файле присутствует только один из ключей!");
                return;
            }

            order = new Order();
        }

        public void AddDataShower(Action<string> action) => _dataFieldShow += action;
        public void AddDataCleaner(Action action) => _dataFieldClean += action;
        public void AddBalanceShow(Action<float> action) => _balanceShow += action;
        public void AddAvailableShow(Action<float> action) => _availableBalanceShow += action;
        public void AddIterMonitoring(Action<int> iter) => _iterMonitoring += iter;

        public void CleanAllEvents()
        {
            _dataFieldShow = null;
            _dataFieldClean = null;
            _balanceShow = null;
            _availableBalanceShow = null;
            _iterMonitoring = null;
        }

        string _restartAndMessage = "\nПожалуйста, попробуйте перезапустить приложение и сообщите разработчику.";

        public void StartMonitoring(CancellationTokenSource cts, int lowBorder)
        {            
            if (lowBorder < 1)
            {
                _dataFieldShow?.Invoke("Ошибка! Нижняя граница < 1!" + _restartAndMessage);
                return;
            }
            this._ct = cts.Token;
            _lowBorder = lowBorder;
            
            closer = new CloseAllPositions();
            balanceFutures = new AccountFutureBalance();

            Thread monitoringProcess = new Thread(new ParameterizedThreadStart(Monitoring));
            _dataFieldShow?.Invoke("\nИспользуемый url: " + url);
            _dataFieldShow?.Invoke("\nЗапущен поток получения данных!");
            monitoringProcess.Start(keys);
            
        }


        private void Monitoring(object o)
        {            
            string[] keys = (string[])o;
            
            string log = "\t\t\tSTART Monitoring at time: "+DateTime.Now.ToString()+"\n";
            int iteration = 1;
            string result;
            
            List<FutureBalance> futureBalances;

            while (!_ct.IsCancellationRequested)
            {
                Thread.Sleep(80);
                                
                result = balanceFutures.GetAccountBalances(url, keys[0], keys[1]);
                futureBalances = JsonConvert.DeserializeObject<List<FutureBalance>>(result);

                if (!IsBalanceOK(futureBalances)) CancelAllOpenOrders();

                _availableBalanceShow?.Invoke(futureBalances[1].availableBalance);
                _balanceShow?.Invoke(futureBalances[1].balance);

                //log += $"\n\t\t\tIteration:{iteration} " + DateTime.Now.ToString() + "\n" + $"Общий баланс:{futureBalances[1].balance} Доступный баланс:{futureBalances[1].availableBalance}\n";

                iteration++;
                _iterMonitoring?.Invoke(iteration);
            }
            log += ("Мониторинг остановлен, итераций: " + iteration);
            log += "\t\t\tEND Monitoring at time: " + DateTime.Now.ToString() + "\n";

            File.AppendAllText(@"_LOG_Monitoring.txt", log);
        }

        private bool IsBalanceOK(List<FutureBalance> balanses)
        {
            
            if (balanses[1].balance < _lowBorder) return false;
            else return true;

        }

        public void CancelAllOpenOrders()
        {
            CloseAllPositions cl = new CloseAllPositions();
            string result = $"\nВнимание, было вызвано закрытие всех позиций! {DateTime.Now}\n";
            
            try
            {
                result += "\nCloseOpenPositions: " + cl.CloseOpenPositions(url, keys[0], keys[1]);
                result += "\n--------------\n";
                result += "\nCloseShort: " + cl.CloseShort(url, keys[0], keys[1]);
                result += "\n--------------\n";
                result += "\nCloseLong: " + cl.CloseLong(url, keys[0], keys[1]);

                _dataFieldClean();
                _dataFieldShow?.Invoke($"\nВНИМАНИЕ, было вызвано закрытие всех позиций! {DateTime.Now}\n");

            }
            catch (Exception ex)
            {
                _dataFieldClean();
                _dataFieldShow?.Invoke("ВНИМАНИЕ!! ПРОИЗОШЛА ПРОИЗОШЛА ОШИБКА ПРИ ПОПЫТКЕ ЗАКРЫТЬ ПОЗИЦИИ!\nЗАКРОЙТЕ ПОЗИЦИИ ВРУЧНУЮ!!");
                result += "\n\n!ОШИБКА! ПРИ ЗАКРЫТИИ!" + ex;
                result += DateTime.Now;
            }

            

            File.AppendAllText("_LOG_CloseAllPositions.txt", result);
        }

        #region TestMethods
        public static string TestGetServerTime()
        {
            string result = "";

            IMarket market = new Market();
            result = market.CheckServerTime();

            return result;
        }

        #endregion
    }
}
