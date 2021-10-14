using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TradeController.Sources.Model
{
    class Asset
    {
        [JsonProperty("asset")]
        public string asset { get; set; }

        [JsonProperty("walletBalance")]
        public float walletBalance { get; set; }

        [JsonProperty("unrealizedProfit")]
        public float unrealizedProfit { get; set; }

        [JsonProperty("marginBalance")]
        public float marginBalance { get; set; }

        [JsonProperty("maintMargin")]
        public float maintMargin { get; set; }
        
        [JsonProperty("initialMargin")]
        public float initialMargin { get; set; }

        [JsonProperty("positionInitialMargin")]
        public float positionInitialMargin { get; set; }

        [JsonProperty("openOrderInitialMargin")]
        public float openOrderInitialMargin { get; set; }

        [JsonProperty("maxWithdrawAmount")]
        public float maxWithdrawAmount { get; set; }

        [JsonProperty("crossWalletBalance")]
        public float crossWalletBalance { get; set; }
        
        [JsonProperty("crossUnPnl")]
        public float crossUnPnl { get; set; }
        
        [JsonProperty("availableBalance")]
        public float availableBalance { get; set; }
        [JsonProperty("marginAvailable")]
        public string marginAvailable { get; set; }
        [JsonProperty("updateTime")]
        public long updateTime { get; set; }


    }
}
