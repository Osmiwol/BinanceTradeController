using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TradeController.Sources.Model
{
    class OpenPosition
    {
        [JsonProperty("orderId")]
        public long orderId { get; set; }
        [JsonProperty("symbol")]
        public string symbol { get; set; }
        [JsonProperty("status")]
        public string status { get; set; }
        [JsonProperty("clientOrderId")]
        public string clientOrderId { get; set; }

        [JsonProperty("price")]
        public float price { get; set; }
        [JsonProperty("avgPrice")]
        public float avgPrice { get; set; }
        [JsonProperty("origQty")]
        public float origQty { get; set; }
        [JsonProperty("executedQty")]
        public float executedQty { get; set; }
        [JsonProperty("cumQuote")]
        public float cumQuote { get; set; }
        [JsonProperty("timeInForce")]
        public string timeInForce { get; set; }
        [JsonProperty("type")]
        public string type { get; set; }

        [JsonProperty("reduceOnly")]
        public string reduceOnly { get; set; }
        [JsonProperty("closePosition")]
        public string closePosition { get; set; }
        [JsonProperty("side")]
        public string side { get; set; }
        [JsonProperty("positionSide")]
        public string positionSide { get; set; }
        [JsonProperty("stopPrice")]
        public float stopPrice { get; set; }
        [JsonProperty("workingType")]
        public string workingType { get; set; }
        [JsonProperty("priceProtect")]
        public string priceProtect { get; set; }
        [JsonProperty("origType")]
        public string origType { get; set; }
        [JsonProperty("time")]
        public long time { get; set; }
        [JsonProperty("updateTime")]
        public long updateTime { get; set; }


    }
}
