using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TradeController.Sources.Common
{
    public class LoggerWriter
    {
        static bool isLoggingOn = false;
        //private static ILogger _logger = LogManager.GetCurrentClassLogger();
        
        public static void SetLogging(bool mode) => isLoggingOn = mode;

        public static void Log(string msg = "\n")
        {
            if (isLoggingOn)
                //_logger.Debug(msg);
                File.AppendAllText("_logs.txt", $"{DateTime.Now} {msg}\n");
        }
    
        public static void LogAndConsole(string msg = "\n")
        {
            Console.WriteLine(msg);
            if(isLoggingOn)
                //_logger.Debug(msg);
                File.AppendAllText("_logs.txt", $"{DateTime.Now} {msg}\n");
        }
    }
}
