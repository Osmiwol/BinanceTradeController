using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TradeController.Sources.Model
{
    class AccountInformation
    {
        
        [JsonProperty("feeTier")]
        public int feeTier { get; set; }
       
        [JsonProperty("canWithdraw")]
        public string canWithdraw { get; set; }
        [JsonProperty("canDeposit")]
        public string canDeposit { get; set; }
        [JsonProperty("updateTime")]
        public string updateTime { get; set; }


        [JsonProperty("totalInitialMargin")]
        public float totalInitialMargin { get; set; }

        [JsonProperty("totalMaintMargin")]
        public float totalMaintMargin { get; set; }
        [JsonProperty("totalWalletBalance")]
        public float totalWalletBalance { get; set; }

        [JsonProperty("totalUnrealizedProfit")]
        public float totalUnrealizedProfit { get; set; }
        
        [JsonProperty("totalMarginBalance")]
        public float totalMarginBalance { get; set; }
        
        [JsonProperty("totalPositionInitialMargin")]
        public float totalPositionInitialMargin { get; set; }
        
        [JsonProperty("totalOpenOrderInitialMargin")]
        public float totalOpenOrderInitialMargin { get; set; }
        
        [JsonProperty("totalCrossWalletBalance")]
        public float totalCrossWalletBalance { get; set; }
        
        [JsonProperty("totalCrossUnPnl")]
        public float totalCrossUnPnl { get; set; }

        [JsonProperty("availableBalance")]
        public float availableBalance { get; set; }
        
        [JsonProperty("maxWithdrawAmount")]
        public float maxWithdrawAmount { get; set; }

        [JsonProperty("assets")]
        public List<Asset> assets;
    }
}
