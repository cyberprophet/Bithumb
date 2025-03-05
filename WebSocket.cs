using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using ShareInvest.Bithumb.EventHandler;
using ShareInvest.Crypto;

using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;

namespace ShareInvest.Bithumb;

public class WebSocket : ShareWebSocket<TickerEventArgs>
{
    public WebSocket() : base("pubwss.bithumb.com/pub/ws")
    {

    }

    /// <summary>
    /// symbols: BTC_KRW, ETH_KRW, …
    /// type: 구독 메시지 종류("ticker" / "transaction" / "orderbookdepth")
    /// (optional)tickTypes: tick 종류 ("30M"/"1H"/"12H"/"24H"/"MID")
    /// </summary>
    public async Task RequestAsync(string type, IEnumerable<string> symbols, params string[] tickTypes)
    {
        await base.RequestAsync(JsonConvert.SerializeObject(new
        {
            type,
            symbols,
            tickTypes
        }));
    }

    public async Task RequestAsync(string type, params string[] symbols)
    {
        await base.RequestAsync(JsonConvert.SerializeObject(new
        {
            type,
            symbols
        }));
    }

    public override async Task RequestAsync(string json)
    {
        await base.RequestAsync(json);
    }

    public override async Task ReceiveAsync()
    {
        while (WebSocketState.Open == Socket.State)
        {
            var buffer = new byte[0x400];

            try
            {
                var res = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);

                var str = Encoding.UTF8.GetString(buffer, 0, res.Count);

                if (string.IsNullOrEmpty(str))
                {
                    continue;
                }
                var jToken = JToken.Parse(str);

                if (string.IsNullOrEmpty(jToken.Value<string>("status")))
                {
                    OnReceiveTicker(str);

                    continue;
                }
                Console.WriteLine(new
                {
                    CryptoExchange = nameof(Bithumb),
                    DateTime.Now,
                    Response = jToken
                });
            }
            catch (Exception exception)
            {
#if DEBUG
                Debug.WriteLine(exception);
#else
                Console.WriteLine(exception);
#endif
            }
        }
        Console.WriteLine(new
        {
            CryptoExchange = nameof(Bithumb),
            DateTime.Now,
            Socket = Socket.State
        });
    }

    public override async Task ConnectAsync(string? token = null, TimeSpan? interval = null)
    {
        await base.ConnectAsync(token, interval: interval ?? TimeSpan.FromMilliseconds(0xFFFFFFFE));
    }

    readonly CancellationTokenSource cts = new();
}