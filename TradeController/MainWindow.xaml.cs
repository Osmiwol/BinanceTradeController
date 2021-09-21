using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
        CancellationTokenSource cts = new CancellationTokenSource();        
        int _lowBorder = -1;
        bool _actionMode = false;
        string _pathToKeys = "";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnAction_Click(object sender, RoutedEventArgs e)
        {
            

            if (!_actionMode)
            {
                Controller controller = new Controller();
                controller.AddDataShower(AddDataToInfo);
                controller.AddDataCleaner(ClearInfo);

                btnAction.Content = "Остановка мониторинга";
                tbInfo.Text = "Процесс мониторинга запущен.\n";
                cts = new CancellationTokenSource();               

                btnChooseKeys.IsEnabled = false;
                btnSaveParameters.IsEnabled = false;
                tbBorder.IsEnabled = false;

                controller.StartMonitoring(cts, _pathToKeys, _lowBorder);
            }
            else
            {
                var userChoise = ShowYNWind("Завершить процесс мониторинга?", "Остановка процесса мониторинга");
                if (userChoise == MessageBoxResult.No) return;
                
                cts.Cancel();
                btnAction.Content = "Запуск мониторинга";

                btnChooseKeys.IsEnabled = true ;
                btnSaveParameters.IsEnabled = true;
                tbBorder.IsEnabled = true ;
                tbInfo.Text = "Процесс мониторинга остановлен.\n";
            }

            _actionMode = !_actionMode;
        }

        private void btnSaveParameters_Click(object sender, RoutedEventArgs e)
        {

            if (StringChecker.IsStringEmpty(_pathToKeys))
            { 
                MessageBox.Show("Не указан путь к файлу с ключами!", "Внимание!", MessageBoxButton.OK);
                return;
            }
            if (StringChecker.IsStringEmpty(tbBorder.Text))
            {
                MessageBox.Show("Введены не все данные!", "Внимание!", MessageBoxButton.OK);
                return;
            }            
            
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

        private void btnChooseKeys_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OPF = new OpenFileDialog();
            OPF.Filter = "Файлы txt|*.txt";
            if (OPF.ShowDialog() == true)
            {
                _pathToKeys = OPF.FileName;
                lblPathToKeys.Content = "Путь к ключам: " + _pathToKeys;
                btnSaveParameters.IsEnabled = true;
            }            
                
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var userChoise = ShowYNWind("Завершить работу приложения?", "Закрытие главного окна программы");
            e.Cancel = (userChoise == MessageBoxResult.No);
        }

        private MessageBoxResult ShowYNWind(string text , string header= "Вы уверены?") => MessageBox.Show(text, header, MessageBoxButton.YesNo);


        private void AddDataToInfo(string data) => tbInfo.Text += data;
        private void ClearInfo() => tbInfo.Text = "";
        
        #region TestMethods
        private void TestMethod()
        {
            string json = Controller.TestGetServerTime();
            tbInfo.Text = json;
        }

        private void TestDataShow()
        {
            Controller controller = new Controller();
            
            controller.AddDataShower(AddDataToInfo);
            controller.AddDataCleaner(ClearInfo);

            //controller.StartCheckBalance(null, "asdfjaskdflj", 9000);            
        }

        #endregion


    }
}
