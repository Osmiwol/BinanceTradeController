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
        
        public void AddDataShower(Action<string> action) => _dataFieldShow += action;
        public void AddDataCleaner(Action action) => _dataFieldClean += action;
        string _restartAndMessage = "\nПожалуйста, попробуйте перезапустить приложение и сообщите разработчику.";


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

            string[] keys = KeysParser.GetKeys(FileReader.GetTextFromFile(pathToKeys));
            if (keys.Length < 2)
            {
                _dataFieldClean();
                _dataFieldShow("Ошибка! В файле присутствует только один из ключей!");
                return;
            }

            _ct = cts.Token;

            IAccountService accountService = new AccountService();
            string result = accountService.GetAccountInformation(keys[0], keys[1]);

            JObject jObject = JObject.Parse(result);
            



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

        /*
public void StartCheckBalance(CancellationTokenSource cts, string api,int lowBorder)
{
    if (cts == null || api == null || api.Length < 5 || lowBorder < 0)
    {
        _dataFieldClean?.Invoke();
        _dataFieldShow?.Invoke("\nОШИБКА! Не удалось запустить мониторинг! \nОдин из параметров не установлен!");
        return;
    }

    IAccountService accService = new AccountService();
    string result = accService.GetAccountInformation(api);

    _dataFieldClean?.Invoke();
    _dataFieldShow?.Invoke(result);

}
*/
        #endregion
    }
}
