using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TradeController.Sources.Model
{
    public class FutureBalance
    {
        [JsonProperty("accountAlias")]
        public string accountAlias { get; set; }

        [JsonProperty("asset")]
        public string asset { get; set; }

        [JsonProperty("balance")]
        public float balance { get; set; }

        [JsonProperty("crossWalletBalance")]
        public float crossWalletBalance { get; set; }

        [JsonProperty("crossUnPnl")]
        public float crossUnPnl { get; set; }

        [JsonProperty("availableBalance")]
        public float availableBalance { get; set; }

        [JsonProperty("maxWithdrawAmount")]
        public float maxWithdrawAmount { get; set; }

        [JsonProperty("marginAvailable")]
        public string marginAvailable { get; set; }

        [JsonProperty("updateTime")]
        public long updateTime { get; set; }


    }
}
