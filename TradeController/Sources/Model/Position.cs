using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TradeController.Sources.Model
{
    class Position
    {

        [JsonProperty("symbol")]
        public string symbol { get; set; }
        [JsonProperty("positionAmt")]
        public float positionAmt { get; set; }
        [JsonProperty("entryPrice")]
        public float entryPrice { get; set; }
        [JsonProperty("markPrice")]
        public float markPrice { get; set; }
        [JsonProperty("unRealizedProfit")]
        public float unRealizedProfit { get; set; }
        [JsonProperty("liquidationPrice")]
        public float liquidationPrice { get; set; }
        [JsonProperty("leverage")]
        public int leverage { get; set; }
        [JsonProperty("maxNotionalValue")]
        public float maxNotionalValue { get; set; }
        [JsonProperty("marginType")]
        public string marginType { get; set; }
        [JsonProperty("isolatedMargin")]
        public float isolatedMargin { get; set; }
        [JsonProperty("isAutoAddMargin")]
        public string isAutoAddMargin { get; set; }
        [JsonProperty("positionSide")]
        public string positionSide { get; set; }
        [JsonProperty("notional")]
        public float notional { get; set; }
        [JsonProperty("isolatedWallet")]
        public float isolatedWallet { get; set; }
        [JsonProperty("updateTime")]
        public long updateTime { get; set; }

        /*
         
         * [{"symbol":"BTCUSDT",
         * "positionAmt":"-0.092",
         * "entryPrice":"54126.0",
         * "markPrice":"53990.51597073",
         * "unRealizedProfit":"12.46453069",
         * "liquidationPrice":"107799.15298805",
         * "leverage":"6",
         * "maxNotionalValue":"20000000",
         * "marginType":"isolated",
         * "isolatedMargin":"4990.06469389",
         * "isAutoAddMargin":"false",
         * "positionSide":"BOTH",
         * "notional":"-4967.12746930",
         * "isolatedWallet":"4977.60016320",
         * "updateTime":1633624865907}]
         * */
    }
}
