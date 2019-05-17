using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ExchangeSharp {

  interface IBalance {
    double QuoteAvailable { get; }
    double QuoteOnOrders { get; }
    double BitcoinValue { get; }
  }

  public class Balance : IBalance {
    [JsonProperty("available")]
    public double QuoteAvailable { get; set; }
    [JsonProperty("onOrders")]
    public double QuoteOnOrders { get; set; }
    [JsonProperty("btcValue")]
    public double BitcoinValue { get; set; }
  }

}
