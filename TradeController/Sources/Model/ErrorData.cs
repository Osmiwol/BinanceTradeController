using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TradeController.Sources.Model
{
    class ErrorData
    {
        [JsonProperty("code")]
        public int code { get; set; }

        [JsonProperty("msg")]
        public string asset { get; set; }
    }
}
