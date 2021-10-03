using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TradeController.Sources.Common
{
    class FileManager
    {
        public static string GetTextFromFile(string path)
        {
            string result = "";
            if (string.IsNullOrEmpty(path)) return result;
            if (!File.Exists(path)) return result;

            FileStream fs = new FileStream(path, FileMode.Open);

            // преобразуем строку в байты
            byte[] array = new byte[fs.Length];
            // считываем данные
            fs.Read(array, 0, array.Length);
            // декодируем байты в строку
            result = System.Text.Encoding.Default.GetString(array);

            return result;
        }
    }
}
