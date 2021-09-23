using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using TradeController.Sources.Common;
using TradeController.Sources.Services.BinancePerpetualFutureAPI;
using TradeController.Sources.Services.BinancePerpetualFutureAPI.Account;

namespace TradeController.Sources
{
    class Controller
    {
        Action<string> _dataFieldShow;
        Action _dataFieldClean;
        CancellationToken _ct;
        IAccountService accountService;
        int _lowBorder = 0;

        public void AddDataShower(Action<string> action) => _dataFieldShow += action;
        public void AddDataCleaner(Action action) => _dataFieldClean += action;
        string _restartAndMessage = "\nПожалуйста, попробуйте перезапустить приложение и сообщите разработчику.";

        string pathToLog = @$"С:\____MYTRADELogs\";
        string fileName = "log.txt";

        public void StartMonitoring(CancellationTokenSource cts, string pathToKeys, int lowBorder)
        {
            
            if (cts == null)
            {
                _dataFieldShow?.Invoke("Ошибка! Токен не был передан!" + _restartAndMessage);
                return;
            }
            if (string.IsNullOrEmpty(pathToKeys))
            {
                _dataFieldShow?.Invoke("Ошибка! Путь к файлу с ключами не был указан!" + _restartAndMessage);
                return;
            }
            if (lowBorder < 1)
            {
                _dataFieldShow?.Invoke("Ошибка! Нижняя граница < 1!" + _restartAndMessage);
                return;
            }
            if (!File.Exists(pathToKeys))
            {
                _dataFieldShow?.Invoke("Ошибка! Файл не существует, по указанному пути!" + _restartAndMessage);
                return;
            }

            string[] keys = KeysParser.GetKeys(FileManager.GetTextFromFile(pathToKeys));
            if (keys.Length < 2)
            {
                _dataFieldClean();
                _dataFieldShow("Ошибка! В файле присутствует только один из ключей!");
                return;
            }
            _lowBorder = lowBorder;
            _ct = cts.Token;

            accountService = new AccountService();
            Thread monitoringProcess = new Thread(new ParameterizedThreadStart(Monitoring));
            _dataFieldShow("\nЗапущен поток получения данных!");
            monitoringProcess.Start(keys);
            
        }

        private void Monitoring(object o)
        {            
            string[] keys = (string[])o;
            JObject accountData;
            string log = "\t\t\tSTART Monitoring at time: "+DateTime.Now.ToString()+"\n";
            int iteration = 1;
            while (!_ct.IsCancellationRequested)
            {
                Thread.Sleep(100);
                accountData = JObject.Parse(accountService.GetAccountInformation(keys[0], keys[1]));                
                log += $"\n\t\t\tIteration:{iteration} " + DateTime.Now.ToString() + "\n" + accountData;
                iteration++;
            }
            log += "\t\t\tEND Monitoring at time: " + DateTime.Now.ToString() + "\n";
            FileManager.WtrineInFile(pathToLog, fileName, log);
        }

        

        public void StopMonitoring()
        {

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
