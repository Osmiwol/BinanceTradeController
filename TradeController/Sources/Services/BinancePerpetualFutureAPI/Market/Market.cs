using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace TradeController.Sources.Services.BinancePerpetualFutureAPI
{
    class Market : IMarket
    {
        string url = "https://fapi.binance.com";
        public string CheckServerTime()
        {
            string result = "";
            string CheckServerTime = @"/fapi/v1/time";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + CheckServerTime);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader sr = new StreamReader(stream);            
            result = sr.ReadToEnd();

            return result;
        }

        public string TestConnectivity()
        {
            string result = "";
            string CheckServerTime = @"/fapi/v1/time";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + CheckServerTime);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            result = sr.ReadToEnd();

            return result;
        }

        public Stream CheckServerTimeStream()
        {            
            string CheckServerTime = @"/fapi/v1/time";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + CheckServerTime);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return response.GetResponseStream();                        
        }

        public Stream TestConnectivityStream()
        {            
            string CheckServerTime = @"/fapi/v1/time";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + CheckServerTime);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            return response.GetResponseStream();            
        }

    }
}
