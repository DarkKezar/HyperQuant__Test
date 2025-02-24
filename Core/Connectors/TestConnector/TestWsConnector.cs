using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Core.Constants;
using Core.DTO;
using Core.Extensions;
using Core.Models;

namespace Core.Connectors.TestConnector;

public class TestWsConnector : ITestWSConnector
{
    private readonly HttpClient _httpClient;
    private readonly ClientWebSocket _webSocket;
    private CancellationTokenSource _cancellationTokenSource;
    
    private readonly Dictionary<int, string?> _chanIdToPair = new Dictionary<int, string?>();
    private readonly Dictionary<int, string?> _chanIdToType = new Dictionary<int, string?>();

    private delegate void EntityProcessor(int channel, object[] data);
    private readonly Dictionary<string, EntityProcessor> _entityProcessors = new();
    
    public event Action<Trade>? NewBuyTrade;
    public event Action<Trade>? NewSellTrade;
    public event Action<Candle>? CandleSeriesProcessing;
    
    public TestWsConnector(ClientWebSocket webSocket, CancellationTokenSource source)
    {
        _webSocket = webSocket;
        _cancellationTokenSource = source;
        
        _entityProcessors.Add(BitfinexApiConstants.WebSocketData.Channels.Trades, ProcessTradeMessage);
        _entityProcessors.Add(BitfinexApiConstants.WebSocketData.Channels.Candles, ProcessCandleMessage);
    }

    public async Task ConnectWebSocketAsync()
    {
        await _webSocket.ConnectAsync(new Uri(BitfinexApiConstants.Urls.WebSocketUrl), 
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
            @event = BitfinexApiConstants.WebSocketData.Events.Subscribe,
            channel = BitfinexApiConstants.WebSocketData.Channels.Trades,
            symbol = pair
        };

        SendWebSocketMessage(JsonSerializer.Serialize(wsMessage), cancellationToken);
    }

    /// <summary>
    /// Отпись от WS события.
    /// </summary>
    /// <param name="pair">Валютная пара, которая более не востребована</param>
    /// <param name="cancellationToken">--||--</param>
    public void UnsubscribeTrades(string pair, CancellationToken cancellationToken = default)
    {
        var tradesId = _chanIdToType
            .Where(kvp => kvp.Value.Equals(BitfinexApiConstants.WebSocketData.Channels.Trades))
            .Select(kvp => kvp.Key)
            .ToList();
        var channelId = _chanIdToPair
            .Where(kvp => tradesId.Contains(kvp.Key))
            .FirstOrDefault(kvp => kvp.Value.Equals(pair)).Key;

        if (channelId > 0)
        {
            _chanIdToPair.Remove(channelId);
            _chanIdToType.Remove(channelId);
            
            var wsMessage = new
            {
                @event = BitfinexApiConstants.WebSocketData.Events.Unsubscribe,
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
            @event = BitfinexApiConstants.WebSocketData.Events.Subscribe,
            channel = BitfinexApiConstants.WebSocketData.Channels.Candles,
            key = query.ToWebSocketParams()
        };

        SendWebSocketMessage(JsonSerializer.Serialize(wsMessage), cancellationToken);
    }

    /// <summary>
    /// Отпись от WS события.
    /// </summary>
    /// <param name="pair">Валютная пара, которая более не востребована</param>
    /// <param name="cancellationToken">--||--</param>
    public void UnsubscribeCandles(string pair, CancellationToken cancellationToken = default)
    {
        var candlesId = _chanIdToType
            .Where(kvp => kvp.Value.Equals(BitfinexApiConstants.WebSocketData.Channels.Candles))
            .Select(kvp => kvp.Key)
            .ToList();
        var channelId = _chanIdToPair
            .Where(kvp => candlesId.Contains(kvp.Key))
            .FirstOrDefault(kvp => kvp.Value.Equals(pair)).Key;

        if (channelId > 0)
        {
            _chanIdToPair.Remove(channelId);
            _chanIdToType.Remove(channelId);
            
            var wsMessage = new
            {
                @event = BitfinexApiConstants.WebSocketData.Events.Unsubscribe,
                chanId = channelId
            };

            SendWebSocketMessage(JsonSerializer.Serialize(wsMessage), cancellationToken);
        }
    }
    
    /// <summary>
    /// Классический способ получать поток и собирать его в сообщение с последующей обработкой
    /// </summary>
    /// <param name="cancellationToken"></param>
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
    
    /// <summary>
    /// Основная функция обработки полученного сообщения, предполагается, что в данном методе происходит фильтрация и
    /// валидация полученного сообщения с последующей отправкой на обработку в конкретный метод
    /// </summary>
    /// <param name="message">Полученное сообщения, по документации представляет или JSON-объект или массив (тоже в JSON формате)</param>
    private void ProcessWebSocketMessage(string message)
    {
        try
        {
            if (message.IsJsonObject())
            {
                ProcessJsonObjectMessage(message);
            }
            else
            {
                ProcessArrayObjectMessage(message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing WebSocket message: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Логика обработки и фольтрации JSON-объектов 
    /// </summary>
    /// <param name="message">JSON-объект представленный в виде строки</param>
    private void ProcessJsonObjectMessage(string message)
    {
        using var doc = JsonDocument.Parse(message);
        var root = doc.RootElement;
                
                
        var eventName = root.GetProperty(BitfinexApiConstants.WebSocketData.ResponseFields.Event).GetString();
        if (!eventName!.Equals(BitfinexApiConstants.WebSocketData.Events.Subscribe)) return;
                
        var channelId = root.GetProperty(BitfinexApiConstants.WebSocketData.ResponseFields.ChannelId).GetInt32();
        var pair = root.GetProperty(BitfinexApiConstants.WebSocketData.ResponseFields.Pair).GetString();
        var channel = root.GetProperty(BitfinexApiConstants.WebSocketData.ResponseFields.Channel).GetString();
                
        _chanIdToPair.Add(channelId, pair);
        _chanIdToType.Add(channelId, channel);
    }
    
    /// <summary>
    /// Логика обработки и фольтрации JSON-массивов 
    /// </summary>
    /// <param name="message">JSON-массив представленный в виде строки</param>
    private void ProcessArrayObjectMessage(string message)
    {
        var objs = JsonSerializer.Deserialize<List<object>>(message);
        var type = _chanIdToType[Convert.ToInt32(objs[0].ToString())];
        var processor = _entityProcessors[type];
        
        // Возможно данную часть можно отрефакторить к более лучшему виду
        // На данный момент часть тех. долга
        if (objs[1].ToString()!.IsArray())
        {
            if (objs.Count == 3)
            {
                var obj = JsonSerializer.Deserialize<object[]>(objs[2].ToString());
                processor(Convert.ToInt32(objs[0].ToString()), obj);
            }
        }
        else
        {
            var objsArray = JsonSerializer.Deserialize<IEnumerable<object[]>>(objs[1].ToString());
            foreach (var obj in objsArray)
            {
                processor(Convert.ToInt32(objs[0].ToString()), obj);
            }
        }
    }
    
    /// <summary>
    /// Метод по обработки данных связанных с сущностью Trade
    /// </summary>
    /// <param name="channel">Номер канала, необъходим для получения названия валютной пары</param>
    /// <param name="data">Пришедшие данные</param>
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
    
    /// <summary>
    /// Метод по обработки данных связанных с сущностью Candle
    /// </summary>
    /// <param name="channel">Номер канала, необъходим для получения названия валютной пары</param>
    /// <param name="data">Пришедшие данные</param>
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
    
    /// <summary>
    /// Служеюный метод для отправки сообщения по WebSocket
    /// </summary>
    /// <param name="message">Сообщения</param>
    /// <param name="cancellationToken">Токен отмены отправки</param>
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