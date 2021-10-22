using System;
using System.Collections.Generic;
using System.Text;

namespace TradeController.Sources.Common
{
    class KeysParser
    {

        public static string[] GetKeys(string text)
        {
            LoggerWriter.LogAndConsole("GetKeys");
            string[] keys = new string[0];
            if (text == null || text.Length < 64) return keys;

            int enterPosition = -1;

            enterPosition = text.IndexOf('\n') - 1;
            string openKey = text.Substring(0, enterPosition);
            string closeKey = text.Substring((enterPosition + 2), text.Length - (enterPosition + 2));

            keys = new string[2] { openKey, closeKey };
            LoggerWriter.LogAndConsole("GetKeys завершен");
            return keys;
        }

    }
}
