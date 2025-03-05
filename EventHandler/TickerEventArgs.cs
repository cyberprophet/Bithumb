using Newtonsoft.Json;

using ShareInvest.Bithumb.Models;

namespace ShareInvest.Bithumb.EventHandler;

public class TickerEventArgs(string json) : EventArgs
{
    public TickerContent? Ticker
    {
        get;
    }
        = JsonConvert.DeserializeObject<RealTicker>(json)?.Content;
}