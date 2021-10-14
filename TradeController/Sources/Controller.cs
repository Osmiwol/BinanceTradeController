﻿using Newtonsoft.Json;
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
        Action<float> _pnl;
        CancellationToken _ct;
        IAccountService accountService;
        int _lowBorder = 0;
        string[] keys;

        IOrder order;

        //костыльно, но работает
        CloseAllPositions closer;
        AccountFutureBalance balanceFutures;
        ServicePositionInformation servicePositionInfo;
        string url = "https://testnet.binancefuture.com"; //тестовый url
        //string url = "https://fapi.binance.com";
        //
        CloseAllPositions cl;
        AccountInformationService accountInfo;
        PositionInformation positionInformation;
        CurrentAllOpenOrders currentAllOpenOrders;

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

            order = new Order();
            cl = new CloseAllPositions();
            cl.SetParameters(url, keys[0], keys[1]);
            currentAllOpenOrders = new CurrentAllOpenOrders();
            currentAllOpenOrders.SetParameters(url, keys[0], keys[1]);
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
            currentAllOpenOrders = new CurrentAllOpenOrders();
            currentAllOpenOrders.SetParameters(url, keys[0], keys[1]);
        }
        int iteration = 1;
        public int GetIterations() => iteration;

        
        private void Monitoring(object o)
        {            
            string[] keys = (string[])o;

            
            balanceFutures = new AccountFutureBalance();
            accountInfo = new AccountInformationService();
            servicePositionInfo = new ServicePositionInformation();
            
            string log = "\t\t\tSTART Monitoring at time: "+DateTime.Now.ToString()+"\n";
            iteration = 1;
            string result;
            
            balanceFutures.SetParameters(url, keys[0], keys[1]);
            accountInfo.SetParameters(url, keys[0], keys[1]);
            servicePositionInfo.SetParameters(url, keys[0], keys[1]);
            positionInformation.SetParameters(url, keys[0], keys[1]);
            currentAllOpenOrders.SetParameters(url, keys[0], keys[1]);
            currentAllOpenOrders = new CurrentAllOpenOrders();
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

        List<Position> positions;
        List<Position> longsForClosing;
        List<Position> shortsForClosing;
        List<OpenOrder> openOrders;
        public void CancelAllOpenOrders()
        {
            string log = $"\nВнимание, было вызвано закрытие всех позиций! {DateTime.Now}\n Мониторинг остановлен!";
            string allPositionsResult;
            longsForClosing = new List<Position>();
            shortsForClosing = new List<Position>();
            openOrders = new List<OpenOrder>();
            try
            {
                allPositionsResult = positionInformation.GetPostitionInformation();
                if(ResponseConverter.IsResponseBalance(allPositionsResult))
                positions = JsonConvert.DeserializeObject<List<Position>>(allPositionsResult);
                else
                {
                    _dataFieldClean?.Invoke();
                    _dataFieldShow?.Invoke("\nВНИМАНИЕ!!НЕ УДАЛОСЬ СЧИТАТЬ ТЕКУЩИЕ  ПОЗИЦИИ ДЛЯ ЗАКРЫТИЯ!\nНЕМЕДЛЕННО ЗАКРОЙТЕ ИХ ВРУЧНУЮ!\n");
                    log += "\nВНИМАНИЕ!!НЕ УДАЛОСЬ СЧИТАТЬ ТЕКУЩИЕ  ПОЗИЦИИ ДЛЯ ЗАКРЫТИЯ!\nНЕМЕДЛЕННО ЗАКРОЙТЕ ИХ ВРУЧНУЮ!\n";
                }
                    
            }
            catch (Exception ex)
            {
                _dataFieldClean?.Invoke();
                _dataFieldShow?.Invoke("\nВНИМАНИЕ!!НЕ УДАЛОСЬ СЧИТАТЬ ТЕКУЩИЕ ПОЗИЦИИ ДЛЯ ЗАКРЫТИЯ!\nНЕМЕДЛЕННО ЗАКРОЙТЕ ИХ ВРУЧНУЮ!\n");
                log += ex;
            }
            
            string openPositionsResult;
           
            openPositionsResult = currentAllOpenOrders.GetCurrentAllOpenOrders();
            if (openPositionsResult.Contains("symbol"))
                openOrders = JsonConvert.DeserializeObject<List<OpenOrder>>(openPositionsResult);
            

            for (int i = 0; i < positions.Count; i++)
            {
                if (positions[i].notional < 0)
                    longsForClosing.Add(positions[i]);
                else if (positions[i].notional > 0)
                    shortsForClosing.Add(positions[i]);
            }

            string result = "";
            string resOpen ="";
            string resShort = "";
            string resLong = "";


            foreach (var item in openOrders)
            {
                resOpen += cl.CloseOpenPositions(item);

                log += $"{DateTime.Now} open: " + resOpen;
                if (ResponseConverter.IsResponseError(result)) ErrorLogic(result);
                else _dataFieldShow?.Invoke("Закрытие Open позиций успешно!\n");
            }

            foreach (Position longPos in longsForClosing)
            {
                resLong = cl.CloseLong(longPos);

                log += $"{DateTime.Now} long: " + resLong;
                if (ResponseConverter.IsResponseError(result)) ErrorLogic(result);
                else _dataFieldShow?.Invoke("Закрытие Long позиции успешно!\n");
            }

            foreach(Position shortPos in shortsForClosing)
            {
                resShort += cl.CloseShort(shortPos);
                log += $"{DateTime.Now} short: " + resShort;
                if (ResponseConverter.IsResponseError(result)) ErrorLogic(result);
                else _dataFieldShow?.Invoke("Закрытие Short позиций успешно!\n");
            }
        

            _dataFieldShow?.Invoke(log);
            
        
            File.AppendAllText("_LOG_CloseAllPositions.txt", log);
            isCloseOrdersWasCalled = true;
        }

        

        private void ErrorLogic(string result)
        {
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


    }
}
