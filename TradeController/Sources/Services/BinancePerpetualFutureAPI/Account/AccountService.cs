using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace TradeController.Sources.Services.BinancePerpetualFutureAPI.Account
{
    class AccountService : IAccountService
    {
        string url = "https://fapi.binance.com";
        public string GetAccountInformation(string api)
        {
            string result = "";
            if (api == null || api.Length < 5)
                return result;

            string CheckServerTime = @$"/fapi/v2/account?timestamp=&signature=&Content-Type=application/json&X-MBX-APIKEY={api}";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + CheckServerTime);
            
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            result = sr.ReadToEnd();

            return result;
        }

        public Stream GetAccountInformationStream(string Api)
        {
            throw new NotImplementedException();
        }
    }
}
