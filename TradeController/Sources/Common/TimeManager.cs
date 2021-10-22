using System;
using System.Collections.Generic;
using System.Text;

namespace TradeController.Sources.Common
{
    class TimeManager
    {
        //public static Int64 GetTimeStamp() => (Int64)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        public static int GetTimeStamp()
        {
            LoggerWriter.LogAndConsole($"GetTimeStamp");
            return (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static long ConvertToUnixTime(DateTime datetime)
        {
            DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return (long)(datetime - sTime).TotalSeconds;
        }
    }
}
