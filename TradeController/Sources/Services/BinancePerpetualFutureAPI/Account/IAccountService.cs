using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TradeController.Sources.Services.BinancePerpetualFutureAPI.Account
{
    interface IAccountService 
    {
        public string GetAccountInformation(string Api);

        public Stream GetAccountInformationStream(string Api);
    }
}
