using System;
using System.Collections.Generic;
using System.Text;

namespace TradeController.Sources.Common
{
    class StringChecker
    {
        public static bool IsStringEmpty(string str)
        {
            if (str == null || str.Length < 1 || str == "")
                return true;
            else
                return false;
        }
    }
}
