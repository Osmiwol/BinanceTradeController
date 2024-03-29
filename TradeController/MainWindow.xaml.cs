﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TradeController.Sources;
using TradeController.Sources.Common;

namespace TradeController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CancellationTokenSource cts;
        int _lowBorder = -1;
        bool _actionMode = false;
        string _pathToKeys = "";
        IController controller;
        public MainWindow()
        {
            InitializeComponent();
            cts = new CancellationTokenSource();
            
        }

        private void btnAction_Click(object sender, RoutedEventArgs e)
        {
            
            
            LoggerWriter.LogAndConsole("Нажата кнопка запуск");

            if (!_actionMode)
            {
                try
                {
                    TimeUpdator.SetTimeToCurrent();
                }
                catch (Exception ex)
                {
                    tbInfo.Text = $"При попытке установки текущего времени возникла ошибка: {ex}\nСвяжитесь с разработчиком!";
                }

                LoggerWriter.LogAndConsole("\nМониторинг запущен");
                cts = new CancellationTokenSource();
                

                controller.AddDataShower(HandlerDataToInfo);
                controller.AddDataCleaner(HandlerClearInfo);

                controller.AddAvailableShow(HandlerAvailableBalance);
                controller.AddBalanceShow(HandlerCommonBalance);
                controller.AddBalancePNL(HandlerPNL);
                controller.AddIterMonitoring(HandlerIter);

                btnAction.Content = "Остановка мониторинга";
                tbInfo.Text = "Процесс мониторинга запущен.\n";
                         

                btnChooseKeys.IsEnabled = false;
                btnSaveParameters.IsEnabled = false;
                tbBorder.IsEnabled = false;
                _lowBorder = int.Parse( tbBorder.Text);

                controller.StartMonitoring(cts, _lowBorder);

            }
            else
            {
                
                var userChoise = ShowYNWind("Завершить процесс мониторинга?", "Остановка процесса мониторинга");
                if (userChoise == MessageBoxResult.No) return;

                LoggerWriter.LogAndConsole("Мониторинг остановлен");
                cts.Cancel();
                btnAction.Content = "Запуск мониторинга";

                btnChooseKeys.IsEnabled = true ;
                btnSaveParameters.IsEnabled = true;
                tbBorder.IsEnabled = true ;
                tbInfo.Text = "Процесс мониторинга остановлен.\n";

                controller.CleanAllEvents();
                LoggerWriter.LogAndConsole("controller.CleanAllEvents");
                lblAvailableBalance.Content = "?";
                lblCommonBalance.Content = "?";

            }

            _actionMode = !_actionMode;
        }

        private void btnSaveParameters_Click(object sender, RoutedEventArgs e)
        {
            LoggerWriter.LogAndConsole("\nСохранение параметров");
            if (string.IsNullOrEmpty(_pathToKeys))
            { 
                MessageBox.Show("Не указан путь к файлу с ключами!", "Внимание!", MessageBoxButton.OK);
                return;
            }
            if (string.IsNullOrEmpty(tbBorder.Text))
            {
                MessageBox.Show("Введены не все данные!", "Внимание!", MessageBoxButton.OK);
                return;
            }
            LoggerWriter.LogAndConsole("\nПараметры сохранены..");
            string lowBorder = tbBorder.Text;
            try
            {
                _lowBorder = int.Parse(lowBorder);
                btnAction.IsEnabled = true;
            }
            catch(Exception ex)
            {
                MessageBox.Show("В поле \"Нижний порог баланса\" введены некорректные данные!", "Внимание!", MessageBoxButton.OK);
                btnAction.IsEnabled = false;
                return;
            }

            
        }
        string oldNameKeysFile = "";
        private void btnChooseKeys_Click(object sender, RoutedEventArgs e)
        {
            LoggerWriter.LogAndConsole("Нажата кнопка выбора пути к ключам");
            OpenFileDialog OPF = new OpenFileDialog();
            OPF.Filter = "Файлы txt|*.txt";
            if (OPF.ShowDialog() == true)
            {
                if (oldNameKeysFile == OPF.FileName) return;
                _pathToKeys = OPF.FileName;
                oldNameKeysFile = _pathToKeys;
                lblPathToKeys.Content = "Путь к ключам: " + _pathToKeys;
                LoggerWriter.LogAndConsole("Путь к ключам = " + _pathToKeys);
                LoggerWriter.LogAndConsole("Инициализируется экземпляр controller..");
                controller = new Controller(cts, _pathToKeys);
                LoggerWriter.LogAndConsole("Экземпляр controller инициализирован");
                btnSaveParameters.IsEnabled = true;
                btnCloseAllDeals.IsEnabled = true;
            
            }
            OPF = null;
        }

        private void btnCloseAllDeals_Click(object sender, RoutedEventArgs e)
        {

            if (ShowYNWind("Вы уверены, что хотите закрыть все позиции?", "Закрыть все позиции?") == MessageBoxResult.Yes)
            {
                tbInfo.Text += "\nМетод закрытия сделок вызван вручную!";
                LoggerWriter.LogAndConsole("Метод закрытия сделок вызван вручную!");
                controller.CancelAllOpenOrders();
            }
            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LoggerWriter.LogAndConsole("Вызвано закрытие программы!");
            var userChoise = ShowYNWind("Завершить работу приложения?", "Закрытие программу");
            e.Cancel = (userChoise == MessageBoxResult.No);
            Thread.Sleep(500);
            
            cts.Cancel();
            LoggerWriter.LogAndConsole("----ПРОГРАММА ДОЛЖНА БЫТЬ ЗАКРЫТА!----");
        }

        private MessageBoxResult ShowYNWind(string text , string header= "Вы уверены?") => MessageBox.Show(text, header, MessageBoxButton.YesNo);

        private void HandlerDataToInfo(string data) {
            try 
            { 
                Dispatcher.Invoke(() => tbInfo.Text += data);
            }
            catch(Exception ex)
            {
                LoggerWriter.LogAndConsole( $"\n{DateTime.Now} Произошла ошибка при попытке вывода общей инофрмации!" + ex);
            }
}
        private void HandlerClearInfo() {
            try
            { 
                Dispatcher.Invoke(() => tbInfo.Text = "");
            }
            catch(Exception ex)
            {
                LoggerWriter.LogAndConsole( $"\n{DateTime.Now} Произошла ошибка при попытке очистки поля с информацией !" + ex);
            }
}

        public void HandlerCommonBalance(float balance)
        {
            try
            { 
                Dispatcher.Invoke(() => lblCommonBalance.Content = balance);
            }
            catch(Exception ex)
            {
                LoggerWriter.LogAndConsole( $"\n{DateTime.Now} Произошла ошибка при попытке вывода общего баланса!" + ex);
            }
        }


        public void HandlerIter(int iter)
        {
            try { 
            Dispatcher.Invoke(() => lblIter.Content = iter);
                
            }
            catch(Exception ex)
            {
                LoggerWriter.LogAndConsole( $"\n{DateTime.Now} Произошла ошибка при попытке вывода значения итераций!" + ex);
            }
}

        public void HandlerAvailableBalance(float availableBalance)
        {
            try
            {
                Dispatcher.Invoke(() => lblAvailableBalance.Content = availableBalance);
            }
            catch(Exception ex)
            {
                LoggerWriter.LogAndConsole( $"\n{DateTime.Now} Произошла ошибка при попытке вывода доступного баланса!" + ex);
            }
        }

        public void HandlerPNL(float pnl)
        {
            try
            {
                Dispatcher.Invoke(() => lblPNL.Content = pnl);
            }
            catch (Exception ex)
            {
                LoggerWriter.LogAndConsole( $"\n{DateTime.Now} Произошла ошибка при попытке вывода доступного баланса!" + ex);
            }
        }


        private void tbBorder_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            
            if (!int.TryParse(e.Text, out int i))e.Handled = true;
        }

        private void chbLogs_Checked(object sender, RoutedEventArgs e)
        {
            
        }

        private void chbLogs_Click(object sender, RoutedEventArgs e)
        {
            if (chbLogs.IsChecked == true)
                LoggerWriter.SetLogging(true);
            else LoggerWriter.SetLogging(false);
        }
    }
}
