using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using TradeController.Sources.Common;
using TradeController.Sources.DealHelper;
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
        Action<float> _pnl;
        CancellationToken _ct;
        
        int _lowBorder = 0;
        string[] keys;

        IOrder order;

        DealCloser dealCloser;

        //костыльно, но работает
        CloseAllPositions closer;
        
        
        //string url = "https://testnet.binancefuture.com"; //тестовый url
        string url = "https://fapi.binance.com";
        //
        CloseAllPositions cl;
        AccountInformationService accountInfo;
        PositionInformation positionInformation;
        

        bool isCloseOrdersWasCalled = false;

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

            
            cl = new CloseAllPositions();
            cl.SetParameters(url, keys[0], keys[1]);

            dealCloser = new DealCloser(url, keys);
            positionInformation = new PositionInformation();
            positionInformation.SetParameters(url, keys[0], keys[1]);
        }

        public void AddDataShower(Action<string> action) => _dataFieldShow += action;
        public void AddDataCleaner(Action action) => _dataFieldClean += action;
        public void AddBalanceShow(Action<float> action) => _balanceShow += action;
        public void AddAvailableShow(Action<float> action) => _availableBalanceShow += action;
        public void AddIterMonitoring(Action<int> iter) => _iterMonitoring += iter;

        public void AddBalancePNL(Action<float> action) => _pnl += action;

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
            positionInformation = new PositionInformation();
            Thread monitoringProcess = new Thread(new ParameterizedThreadStart(Monitoring));
            _dataFieldShow?.Invoke("\nИспользуемый url: " + url);
            _dataFieldShow?.Invoke("\nЗапущен поток получения данных!");
            monitoringProcess.Start(keys);

        }
        int iteration = 1;
        public int GetIterations() => iteration;

        
        private void Monitoring(object o)
        {            
            string[] keys = (string[])o;


            positionInformation.SetParameters(url, keys[0], keys[1]);
            accountInfo = new AccountInformationService();
            
            string log = "\t\t\tSTART Monitoring at time: "+DateTime.Now.ToString()+"\n";
            iteration = 1;
            string result;
            
            accountInfo.SetParameters(url, keys[0], keys[1]);            
            positionInformation.SetParameters(url, keys[0], keys[1]);

            float futureBalance = -1;
            float pnl = 0;
            float balance = 0 ;
            AccountInformation accInfo;

            while (!_ct.IsCancellationRequested)
            {
                if (isCloseOrdersWasCalled)
                {                    
                    CancelAllOpenOrders();
                    _availableBalanceShow?.Invoke(futureBalance);
                    _pnl?.Invoke(pnl);
                    _balanceShow?.Invoke(balance);

                    if (DateTime.Now.Second == 0) iteration = 0;
                    iteration++;
                    _iterMonitoring?.Invoke(iteration);                    
                }
                
                result = accountInfo.GetAccountBalances();

                if (ResponseConverter.IsResponseBalance(result))
                {                    
                    accInfo = JsonConvert.DeserializeObject<AccountInformation>(result);
                    pnl = accInfo.totalUnrealizedProfit;
                    balance = accInfo.totalWalletBalance;
                    futureBalance = pnl + balance;
                    
                    if (futureBalance < _lowBorder)
                    {
                        CancelAllOpenOrders();
                        _dataFieldShow?.Invoke($"\nВНИМАНИЕ!Трейдер привысил допустимый предел трат!\n {DateTime.Now}\n");

                        _availableBalanceShow?.Invoke(futureBalance);
                        _pnl?.Invoke(pnl);
                        _balanceShow?.Invoke(balance);

                        CancelAllOpenOrders();

                        //break;
                    }

                }
                else                     
                {
                    if ((ResponseConverter.IsResponseError(result)))
                    {
                        ErrorLogic(result);
                        break;
                    }
                    else
                    {
                        _dataFieldClean?.Invoke();
                        _dataFieldShow?.Invoke("\nНЕИЗВЕСТНАЯ ОШИБКА! Сообщите разработчику:\n" + result );
                        log += "\nНЕИЗВЕСТНАЯ ОШИБКА!Сообщите разработчику:\n" + result;
                    }
                }

                _availableBalanceShow?.Invoke(futureBalance);
                _pnl?.Invoke(pnl);
                _balanceShow?.Invoke(balance);

                if (DateTime.Now.Second == 0) iteration = 0;
                iteration++;
                _iterMonitoring?.Invoke(iteration);

            }
            _dataFieldShow?.Invoke("\nМониторинг остановлен, итераций: " + iteration);
            log += ("Мониторинг остановлен, итераций: " + iteration);
            log += "\t\t\tEND Monitoring at time: " + DateTime.Now.ToString() + "\n";

            File.AppendAllText(@"_LOG_Monitoring.txt", log);
        }


        
        public void CancelAllOpenOrders()
        {
            
            string resultOpenDeals = dealCloser.CloseOpenDeals();
            if(resultOpenDeals.Contains("code") && resultOpenDeals.Contains("msg") && resultOpenDeals.Contains("200"))
                _dataFieldShow?.Invoke($"\nВНИМАНИЕ! Трейдер привысил допустимый предел трат! \nОткрытые заявки были закрыты!:{DateTime.Now}\n");
            else if (resultOpenDeals.Contains("code") && resultOpenDeals.Contains("msg") ) ErrorLogic(resultOpenDeals);
            else ErrorLogic(resultOpenDeals);

            List<Position> positions = new List<Position>();            
            
            string allPositionsData = positionInformation.GetPostitionInformation();
            positions = PositionsToClosing(allPositionsData);
            if (positions.Count < 1) return;
            string closePositionsResult = dealCloser.CloseDeals(positions);
            if (closePositionsResult != "")
                _dataFieldShow?.Invoke($"\nВНИМАНИЕ! Трейдер привысил допустимый предел трат! \nФьючерсные заявки были закрыты! :{DateTime.Now}\n");
            else if (closePositionsResult.Contains("code") && closePositionsResult.Contains("msg"))
                ErrorLogic(closePositionsResult);
        }

        private List<Position> PositionsToClosing(string positionsData)
        {
            List<Position> result = new List<Position>();
            if (string.IsNullOrEmpty(positionsData)) return result;

            List<Position> positions = JsonConvert.DeserializeObject<List<Position>>(positionsData);

            for (int i = 0; i < positions.Count; i++)
            {
                if (positions[i].notional != 0)
                    result.Add(positions[i]);
            }

            return result;
        }


        private void ErrorLogic(string result)
        {
            if (string.IsNullOrEmpty(result)) return;
            
            if (!(result.Contains("code") && result.Contains("msg"))) return;
            ErrorData error = JsonConvert.DeserializeObject<ErrorData>(result);
            
            _dataFieldShow?.Invoke($"\nВНИМАНИЕ, ОШИБКА!:\n");
            _dataFieldShow?.Invoke($"Код ошибки: { error.code}\n Текст ошибки: { error.asset}\n");
            switch (error.code)
            {
                case (-1021):
                    _dataFieldShow?.Invoke($"Рекомендация: попробуйте синхронизировать время системы и перезапустить ПО.\n");
                    break;
                case (-2014):
                    _dataFieldShow?.Invoke($"Рекомендация: попробуйте изменить ключи подключения.\nКлучи должны быть сохранены в формате: \nopenKey\ncloseKey(без переноса строки)\n");
                    break;
                case (-2015):
                    _dataFieldShow?.Invoke($"Рекомендация: попробуйте изменить ключи подключения.\nКлучи должны быть сохранены в формате: \nopenKey\ncloseKey(без переноса строки)\n");
                    break;
                case (-2022):
                    _dataFieldShow?.Invoke($"Вероятно, все в порядке. Если был открыт только Long/Short это сообщение,увы, будет появляться.\n");
                    break;
                case (-429):
                    _dataFieldShow?.Invoke($"Время появляения {DateTime.Now}\nРекомендация: подождать пару минут и перезапустить ПО, иначе бан.\n");
                    break;
                case (429):
                    _dataFieldShow?.Invoke($"Время появляения {DateTime.Now}\nРекомендация: подождать пару минут и перезапустить ПО, иначе бан.\n");
                    break;
                default:
                    _dataFieldShow?.Invoke($"Рекомендация: сообщите код ошибки разработчику и попробуйте перезапустить ПО.\n");
                    break;
            }
        }

        //MethodForTest
        private void TestMethod()
        {
            DealCloser dealCloser = new DealCloser(url, keys);

            List<Position> positions = new List<Position>();
            positionInformation = new PositionInformation();
            positionInformation.SetParameters(url, keys[0], keys[1]);

            string allPositionsData = positionInformation.GetPostitionInformation();
            positions = PositionsToClosing(allPositionsData);

            Console.WriteLine(  dealCloser.CloseDeals(positions) );

            /*
            DealCloser dealCloser = new DealCloser(url, keys);
            Console.WriteLine(dealCloser.CloseOpenDeals());
            */

            /*
            _dataFieldShow += ShowMessage;
            IOrder order = new Order(url, keys[0], keys[1]);
            List<OpenPosition> openPositions = new List<OpenPosition>();

            string orders = order.CurrentAllOpenOrders();
            if (orders.Contains("code") && orders.Contains("msg"))
                ErrorLogic(orders);
            else
                openPositions = JsonConvert.DeserializeObject<List<OpenPosition>>(orders);

            for (int i = 0; i < openPositions.Count; i++)
            {
                string response = order.CancelAllOpenOrders(openPositions[i].symbol);

                if (response.Contains("code") && response.Contains("msg") && response.Contains("200"))
                    Console.WriteLine(response);
                else
                    ErrorLogic(response);
            }
            */

        }
        private void ShowMessage(string message)
        {
            Console.WriteLine(message);
        }

       
    }
}
