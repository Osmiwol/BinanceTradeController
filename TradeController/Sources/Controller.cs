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
        bool isTimeError = false;
        public Controller(CancellationTokenSource cts, string pathToKeys)
        {
            LoggerWriter.LogAndConsole($"Вызван конструктор Contoller! Рабочий url: {url}");

            if (cts == null)
            {
                _dataFieldShow?.Invoke("Ошибка! Токен не был передан!" + _restartAndMessage);
                LoggerWriter.LogAndConsole("\nОшибка! Токен не был передан!");
                return;
            }
            _ct = cts.Token;

            if (string.IsNullOrEmpty(pathToKeys))
            {
                _dataFieldShow?.Invoke("Ошибка! Путь к файлу с ключами не был указан!" + _restartAndMessage);
                LoggerWriter.LogAndConsole("\nОшибка! Путь к файлу с ключами не был указан!");
                return;
            }
            
            if (!File.Exists(pathToKeys))
            {
                _dataFieldShow?.Invoke("Ошибка! Файл не существует, по указанному пути!" + _restartAndMessage);
                LoggerWriter.LogAndConsole("\nОшибка! Файл не существует, по указанному пути!");
                return;
            }
            keys = KeysParser.GetKeys(FileManager.GetTextFromFile(pathToKeys));
            if (keys.Length < 2)
            {
                _dataFieldClean();
                _dataFieldShow("Ошибка! В файле присутствует только один из ключей!");
                LoggerWriter.LogAndConsole("\nОшибка! В файле присутствует только один из ключей!");
                return;
            }

            LoggerWriter.LogAndConsole("\nИнициализировано создание Controller");
            cl = new CloseAllPositions();
            cl.SetParameters(url, keys[0], keys[1]);

            dealCloser = new DealCloser(url, keys);
            positionInformation = new PositionInformation();
            positionInformation.SetParameters(url, keys[0], keys[1]);
            LoggerWriter.LogAndConsole("\nController успешно инициализирован!");
        }

        public void AddDataShower(Action<string> action) => _dataFieldShow += action;
        public void AddDataCleaner(Action action) => _dataFieldClean += action;
        public void AddBalanceShow(Action<float> action) => _balanceShow += action;
        public void AddAvailableShow(Action<float> action) => _availableBalanceShow += action;
        public void AddIterMonitoring(Action<int> iter) => _iterMonitoring += iter;

        public void AddBalancePNL(Action<float> action) => _pnl += action;

        public void CleanAllEvents()
        {
            LoggerWriter.LogAndConsole("Вызван CleanAllEvents!");
            _dataFieldShow = null;
            _dataFieldClean = null;
            _balanceShow = null;
            _availableBalanceShow = null;
            _iterMonitoring = null;
            LoggerWriter.LogAndConsole("CleanAllEvents завершен успешно!");
        }

        string _restartAndMessage = "\nПожалуйста, попробуйте перезапустить приложение и сообщите разработчику.";

        public void StartMonitoring(CancellationTokenSource cts, int lowBorder)
        {
            LoggerWriter.LogAndConsole("Вызван StartMonitoring");
            if (lowBorder < 1)
            {
                _dataFieldShow?.Invoke("Ошибка! Нижняя граница < 1!" + _restartAndMessage);
                return;
            }

            LoggerWriter.LogAndConsole("Успешно вызван starMonitoring");
            this._ct = cts.Token;
            _lowBorder = lowBorder;
            
            closer = new CloseAllPositions();
            positionInformation = new PositionInformation();
            Thread monitoringProcess = new Thread(new ParameterizedThreadStart(Monitoring));
            _dataFieldShow?.Invoke("\nИспользуемый url: " + url);
            _dataFieldShow?.Invoke("\nЗапущен поток получения данных!");
            LoggerWriter.LogAndConsole("Данные в функции StartMonitoring успешно проинициализированы!");
            monitoringProcess.Start(keys);
            LoggerWriter.LogAndConsole("Инициализирован поток мониторинга!");

        }
        int iteration = 1;
        public int GetIterations() => iteration;

        
        private void Monitoring(object o)
        {
            
            string[] keys = (string[])o;


            positionInformation.SetParameters(url, keys[0], keys[1]);
            accountInfo = new AccountInformationService();
            
            string log = "\t\t\tSTART Monitoring at time: "+DateTime.Now.ToString()+"\n";
            
            LoggerWriter.LogAndConsole(log);
            iteration = 1;
            string result;

            
            accountInfo.SetParameters(url, keys[0], keys[1]);            
            positionInformation.SetParameters(url, keys[0], keys[1]);


            float futureBalance = -1;
            float pnl = 0;
            float balance = 0 ;
            AccountInformation accInfo;
            int counterTimeError = 0;
            LoggerWriter.LogAndConsole("Запускается цикл мониторинга баланса!");
            while (!_ct.IsCancellationRequested)
            {
                
                if (isCloseOrdersWasCalled)
                {
                    LoggerWriter.LogAndConsole("Было вызвано закрытие сделок в цикле мониторинга! isCloseOrdersWasCalled");
                    CancelAllOpenOrders();
                    _availableBalanceShow?.Invoke(futureBalance);
                    _pnl?.Invoke(pnl);
                    _balanceShow?.Invoke(balance);

                    

                    if (DateTime.Now.Second == 0) iteration = 0;
                    iteration++;
                    _iterMonitoring?.Invoke(iteration);
                    LoggerWriter.LogAndConsole("isCloseOrdersWasCalled Завершено успешно!");
                }
                LoggerWriter.LogAndConsole("Происходит получение баланса фьючерсного аккаунта");
                result = accountInfo.GetAccountBalances();

                if (ResponseConverter.IsResponseBalance(result))
                {
                    LoggerWriter.LogAndConsole($"В цикле балансва полученный ответ вернул состояние баланса");
                    isTimeError = false;
                    counterTimeError = 0;
                    try
                    {
                        LoggerWriter.LogAndConsole($"Попытка сконвертировать баланс");
                        accInfo = JsonConvert.DeserializeObject<AccountInformation>(result);

                        pnl = accInfo.totalUnrealizedProfit;
                        balance = accInfo.totalWalletBalance;
                        futureBalance = pnl + balance;
                        LoggerWriter.LogAndConsole($"Фьючерсный баланс(Основной): {futureBalance} PNL: {pnl} общий баланс: {balance}");
                        if (futureBalance < _lowBorder)
                        {
                            CancelAllOpenOrders();
                            _dataFieldShow?.Invoke($"\nВНИМАНИЕ!Трейдер привысил допустимый предел трат! {DateTime.Now}");
                            LoggerWriter.LogAndConsole($"ВНИМАНИЕ!Трейдер привысил допустимый предел трат! {DateTime.Now}");
                            _availableBalanceShow?.Invoke(futureBalance);
                            _pnl?.Invoke(pnl);
                            _balanceShow?.Invoke(balance);

                            CancelAllOpenOrders();

                            //break;
                        }
                    }
                    catch(Exception ex)
                    {
                        _dataFieldShow("\nВнимание! Произошла непредвиденная ошибка при попытке получить значение баланса!");
                        LoggerWriter.LogAndConsole($"Внимание! Произошла непредвиденная ошибка при попытке получить значение баланса!");
                    }
                    

                }
                else                     
                {
                    if ((ResponseConverter.IsResponseError(result)))
                    {

                        ErrorLogic(result);
                        if (isTimeError && counterTimeError < 70)
                        {
                            LoggerWriter.LogAndConsole($"Внимание! Произошла ошибка синхронизации времени! Итерация {counterTimeError}");
                            counterTimeError++;
                            continue;
                        }
                        else
                        {
                            _dataFieldShow("\nВ цикле мониторинга баланса Произошла ошибка, которую не удалось исправить!\n");
                            LoggerWriter.LogAndConsole(" В цикле мониторинга баланса Произошла ошибка, которую не удалось исправить!");
                            break;
                        }
                    }
                    else
                    {
                        _dataFieldClean?.Invoke();
                        _dataFieldShow?.Invoke("\nНЕИЗВЕСТНАЯ ОШИБКА! Сообщите разработчику:\n" + result );
                        
                        log += "\nНЕИЗВЕСТНАЯ ОШИБКА!Сообщите разработчику:\n" + result;
                        LoggerWriter.LogAndConsole(log + result);
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

            LoggerWriter.LogAndConsole( log);
            
        }


        
        public void CancelAllOpenOrders()
        {
            LoggerWriter.LogAndConsole("Вызван метод CancelAllOpenOrders");
            string resultOpenDeals = dealCloser.CloseOpenDeals();
            if(resultOpenDeals.Contains("code") && resultOpenDeals.Contains("msg") && resultOpenDeals.Contains("200"))
                _dataFieldShow?.Invoke($"\nВНИМАНИЕ! Трейдер привысил допустимый предел трат! \nОткрытые заявки были закрыты!:{DateTime.Now}\n");

            else if (resultOpenDeals.Contains("code") && resultOpenDeals.Contains("msg") ) ErrorLogic(resultOpenDeals);
            else ErrorLogic(resultOpenDeals);

            List<Position> positions = new List<Position>();
            LoggerWriter.LogAndConsole("Получение данных по позициям");
            string allPositionsData = positionInformation.GetPostitionInformation();
            positions = PositionsToClosing(allPositionsData);
            if (positions.Count < 1)
            {
                LoggerWriter.LogAndConsole(" Количество позиций Positions.Count < 1,а, следовательно, закрывать нечего..");
                return;
            }
            else
            {
                LoggerWriter.LogAndConsole($"Закрытие открытых позиций в цикле, количество позиций: {positions.Count}");
                while (positions.Count > 0)
                {

                    string closePositionsResult = dealCloser.CloseDeals(positions);
                    if (closePositionsResult != "")
                        _dataFieldShow?.Invoke($"\nВНИМАНИЕ! Трейдер привысил допустимый предел трат! \nФьючерсные заявки были закрыты! :{DateTime.Now}\n");
                    else if (closePositionsResult.Contains("code") && closePositionsResult.Contains("msg"))
                        ErrorLogic(closePositionsResult);

                    allPositionsData = positionInformation.GetPostitionInformation();
                    
                    positions = PositionsToClosing(allPositionsData);
                    LoggerWriter.LogAndConsole($"результат после попытки закрытия позиций: \n{positions}");
                }

            }
            
        }

        private List<Position> PositionsToClosing(string positionsData)
        {
            LoggerWriter.LogAndConsole("Вызван PositionsToClosing! Парсинг позиций для закрытия из полученных данных");
            List<Position> result = new List<Position>();
            if (string.IsNullOrEmpty(positionsData)) return result;
            List<Position> positions = new List<Position>();
            try
            {
                positions = JsonConvert.DeserializeObject<List<Position>>(positionsData);
            }
            catch(Exception ex)
            {
                LoggerWriter.LogAndConsole($"ВНИМАНИЕ!Парсинг позиций для закрытия из полученных данных не был успешен! {ex}");
                _dataFieldShow?.Invoke(ex.ToString());
                return result;
            }

            for (int i = 0; i < positions.Count; i++)
            {
                if (positions[i].notional != 0)
                    result.Add(positions[i]);
            }
            LoggerWriter.LogAndConsole($"Получены позиции для закрытия! Количество позиций: {positions.Count}");

            return result;
        }

        
        private void ErrorLogic(string result)
        {
            LoggerWriter.LogAndConsole($"Вызван ErrorLogic!");
            if (string.IsNullOrEmpty(result)) return;
            
            if (!(result.Contains("code") && result.Contains("msg"))) return;
            ErrorData error = new ErrorData();
            try
            {
                error = JsonConvert.DeserializeObject<ErrorData>(result);
            }
            catch(Exception ex)
            {
                LoggerWriter.LogAndConsole($"ВНИМАНИЕ!При попытке спарсить ошибку произошла ошибка!: {ex}");
            }
            _dataFieldShow?.Invoke($"\nВНИМАНИЕ, ОШИБКА!:\n");
            _dataFieldShow?.Invoke($"Код ошибки: { error.code}\n Текст ошибки: { error.asset}\n");
            switch (error.code)
            {
                case (-1021):
                    //_dataFieldShow?.Invoke($"Рекомендация: попробуйте синхронизировать время системы и перезапустить ПО.\n");

                    isTimeError = true;
                    _dataFieldShow?.Invoke($"Рекомендация: попробуем синхронизировать время системы и повторить запрос...\n");
                    LoggerWriter.LogAndConsole($"Рекомендация: попробуем синхронизировать время системы и повторить запрос...");
                    TimeUpdator.SetTimeToCurrent();
                    break;
                case (-2014):
                    isTimeError = false;
                    _dataFieldShow?.Invoke($"Рекомендация: попробуйте изменить ключи подключения.\nКлучи должны быть сохранены в формате: \nopenKey\ncloseKey(без переноса строки)\n");
                    LoggerWriter.LogAndConsole($"Рекомендация: попробуйте изменить ключи подключения.\nКлучи должны быть сохранены в формате: \nopenKey\ncloseKey(без переноса строки)");
                    break;
                case (-2015):
                    isTimeError = false;
                    _dataFieldShow?.Invoke($"Рекомендация: попробуйте изменить ключи подключения.\nКлучи должны быть сохранены в формате: \nopenKey\ncloseKey(без переноса строки)\n");
                    LoggerWriter.LogAndConsole($"Рекомендация: попробуйте изменить ключи подключения.\nКлучи должны быть сохранены в формате: \nopenKey\ncloseKey(без переноса строки)");
                    break;
                case (-2022):                    
                    _dataFieldShow?.Invoke($"Вероятно, все в порядке. Если был открыт только Long/Short это сообщение,увы, будет появляться.\n");
                    LoggerWriter.LogAndConsole($"Вероятно, все в порядке. Если был открыт только Long/Short это сообщение,увы, будет появляться.\n");
                    break;
                case (-429):
                    isTimeError = true;
                    _dataFieldShow?.Invoke($"Время появляения {DateTime.Now}\nРекомендация: подождать пару минут и перезапустить ПО, иначе бан.\nВыполняется ожидание..");
                    LoggerWriter.LogAndConsole($"Время появляения {DateTime.Now}\nРекомендация: подождать пару минут и перезапустить ПО, иначе бан.\nВыполняется ожидание..");
                    Thread.Sleep(90 * 1000);
                    break;
                case (429):
                    isTimeError = true;
                    _dataFieldShow?.Invoke($"Время появляения {DateTime.Now}\nРекомендация: подождать пару минут и перезапустить ПО, иначе бан.\nВыполняется ожидание..");
                    LoggerWriter.LogAndConsole($"Время появляения {DateTime.Now}\nРекомендация: подождать пару минут и перезапустить ПО, иначе бан.\nВыполняется ожидание..");
                    Thread.Sleep(90 * 1000);
                    break;
                default:
                    isTimeError = false;
                    _dataFieldShow?.Invoke($"Рекомендация: сообщите код ошибки разработчику и попробуйте перезапустить ПО.\n");
                    LoggerWriter.LogAndConsole($"Рекомендация: сообщите код ошибки разработчику и попробуйте перезапустить ПО.");
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
