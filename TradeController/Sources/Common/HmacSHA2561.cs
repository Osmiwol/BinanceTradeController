﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TradeController.Sources.Common
{
    class HmacSHA256
    {
        public static string SighText(string text, string key)
        {
            LoggerWriter.LogAndConsole("SighText");
            string result = "";

            byte[] byteKey = System.Text.Encoding.UTF8.GetBytes(key);
            // Initialize the keyed hash object.
            using (HMACSHA256 hmac = new HMACSHA256(byteKey))
            {
                byte[] byteArray = Encoding.ASCII.GetBytes(text);
                using (MemoryStream stream = new MemoryStream(byteArray))
                {
                    byte[] arr = hmac.ComputeHash(stream);
                    result = Serialize(arr);
                }
            }

            LoggerWriter.LogAndConsole("SighText завершен");

            return result;
        }



        public static string Serialize(byte[] data)
        {
            LoggerWriter.LogAndConsole("Serialize");
            StringBuilder result = new StringBuilder();

            foreach (byte b in data)
                result.Append(string.Format("{0:x2}", b));

            return result.ToString();
        }

        internal static string SighText(object p, string closeKey)
        {
            throw new NotImplementedException();
        }
    }
}
