using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Core.DTO;
using Core.Models;

namespace Core.Connectors.TestConnector;

public class TestWsConnector : ITestWSConnector
{
    private readonly HttpClient _httpClient;
    private readonly ClientWebSocket _webSocket;
    private CancellationTokenSource _cancellationTokenSource;
    private readonly Dictionary<int, string?> _chanIdToPair = new Dictionary<int, string?>();
    private readonly Dictionary<int, string?> _chanIdToType = new Dictionary<int, string?>();
    
    private const string WebSocketUrl = "wss://api-pub.bitfinex.com/ws/2";
    
    public event Action<Trade>? NewBuyTrade;
    public event Action<Trade>? NewSellTrade;
    public event Action<Candle>? CandleSeriesProcessing;
    
    public TestWsConnector(ClientWebSocket webSocket, CancellationTokenSource source)
    {
        _webSocket = webSocket;
        _cancellationTokenSource = source;
    }

    public async Task ConnectWebSocketAsync()
    {
        await _webSocket.ConnectAsync(new Uri(WebSocketUrl), 
            _cancellationTokenSource.Token);
        _ = ReceiveWebSocketMessagesAsync(_cancellationTokenSource.Token);
    }
    
    /// <summary>
    /// Подписывание на WS событие. Возможно стоит добавить проверку подписан ли текущий экземпляр на текущую пару или нет
    /// </summary>
    /// <param name="pair">--||--</param>
    /// <param name="maxCount">В документировании API не найден параметр</param>
    /// <param name="cancellationToken">--||--</param>
    public void SubscribeTrades(string pair, int maxCount = 100, CancellationToken cancellationToken = default)
    {
        var wsMessage = new
        {
            @event = "subscribe",
            channel = "trades",
            symbol = pair
        };

        SendWebSocketMessage(JsonSerializer.Serialize(wsMessage), cancellationToken);
    }

    public void UnsubscribeTrades(string pair, CancellationToken cancellationToken = default)
    {
        var tradesId = _chanIdToType.Where(kvp => kvp.Value.Equals("trades")).Select(kvp => kvp.Key).ToList();
        var channelId = _chanIdToPair.Where(kvp => tradesId.Contains(kvp.Key)).FirstOrDefault(kvp => kvp.Value.Equals(pair)).Key;

        if (channelId > 0)
        {
            _chanIdToPair.Remove(channelId);
            _chanIdToType.Remove(channelId);
            
            var wsMessage = new
            {
                @event = "unsubscribe",
                chanId = channelId
            };

            SendWebSocketMessage(JsonSerializer.Serialize(wsMessage), cancellationToken);
        }
    }

    /// <summary>
    /// Подписывание на WS событие. Возможно стоит добавить проверку подписан ли текущий экземпляр на текущую пару или нет
    /// </summary>
    public void SubscribeCandles(CandleSeriesQuery query, CancellationToken cancellationToken = default)
    {
        var wsMessage = new
        {
            @event = "subscribe",
            channel = "candles",
            key = query.ToWebSocketParams()
        };

        SendWebSocketMessage(JsonSerializer.Serialize(wsMessage), cancellationToken);
    }

    public void UnsubscribeCandles(string pair, CancellationToken cancellationToken = default)
    {
        var candlesId = _chanIdToType.Where(kvp => kvp.Value.Equals("candles")).Select(kvp => kvp.Key).ToList();
        var channelId = _chanIdToPair.Where(kvp => candlesId.Contains(kvp.Key)).FirstOrDefault(kvp => kvp.Value.Equals(pair)).Key;

        if (channelId > 0)
        {
            _chanIdToPair.Remove(channelId);
            _chanIdToType.Remove(channelId);
            
            var wsMessage = new
            {
                @event = "unsubscribe",
                chanId = channelId
            };

            SendWebSocketMessage(JsonSerializer.Serialize(wsMessage), cancellationToken);
        }
    }
    
    
    private async Task ReceiveWebSocketMessagesAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[1024 * 4];

        while (_webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            WebSocketReceiveResult result;
            var message = new List<byte>();

            do
            {
                result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                message.AddRange(new ArraySegment<byte>(buffer, 0, result.Count));
            } while (!result.EndOfMessage);

            var messageString = Encoding.UTF8.GetString(message.ToArray());
            ProcessWebSocketMessage(messageString);
        }
    }

    private void ProcessWebSocketMessage(string message)
    {
        try
        {
            if (message[0] == '{')
            {
                using var doc = JsonDocument.Parse(message);
                var root = doc.RootElement;

                if (!root.GetProperty("event").GetString()!.Equals("subscribed")) return;
                _chanIdToPair.Add(root.GetProperty("chanId").GetInt32(), root.GetProperty("pair").GetString());
                _chanIdToType.Add(root.GetProperty("chanId").GetInt32(), root.GetProperty("channel").GetString());
            }
            else
            {
                var objs = JsonSerializer.Deserialize<List<object>>(message);
                var type = _chanIdToType[Convert.ToInt32(objs[0].ToString())];

                if (type.Equals("trades"))
                {
                    if (Convert.ToString(objs[1])[0] != '[')
                    {
                        if (objs.Count == 3)
                        {
                            var obj = JsonSerializer.Deserialize<object[]>(objs[2].ToString());
                            ProcessTradeMessage(Convert.ToInt32(objs[0].ToString()), obj);
                        }
                    }
                    else
                    {
                        var objsArray = JsonSerializer.Deserialize<IEnumerable<object[]>>(objs[1].ToString());
                        foreach (var obj in objsArray)
                        {
                            ProcessTradeMessage(Convert.ToInt32(objs[0].ToString()), obj);
                        }
                    }
                }
                else
                {
                    if (Convert.ToString(objs[1])[0] != '[')
                    {
                        if (objs.Count == 3)
                        {
                            var obj = JsonSerializer.Deserialize<object[]>(objs[2].ToString());
                            ProcessCandleMessage(Convert.ToInt32(objs[0].ToString()), obj);
                        }
                    }
                    else
                    {
                        var objsArray = JsonSerializer.Deserialize<IEnumerable<object[]>>(objs[1].ToString());
                        foreach (var obj in objsArray)
                        {
                            ProcessCandleMessage(Convert.ToInt32(objs[0].ToString()), obj);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing WebSocket message: {ex.Message}");
        }
    }

    private void ProcessTradeMessage(int channel, object[] data)
    {
        try
        {
            var trade = new Trade(_chanIdToPair[channel], data);
            if (trade.Amount > 0)
            {
                NewBuyTrade?.Invoke(trade);
            }
            else
            {
                NewSellTrade?.Invoke(trade);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void ProcessCandleMessage(int channel, object[] data)
    {
        try
        {
            var candle = new Candle(_chanIdToPair[channel], data);
            CandleSeriesProcessing?.Invoke(candle);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private async void SendWebSocketMessage(string message, CancellationToken cancellationToken)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, cancellationToken);
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _webSocket?.Dispose();
    }
}