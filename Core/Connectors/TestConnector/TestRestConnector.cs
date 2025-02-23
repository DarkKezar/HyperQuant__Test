using System.Text.Json;
using Core.DTO;
using Core.Models;

namespace Core.Connectors.TestConnector;

public class TestRestConnector : ITestRestConnector
{
    private readonly HttpClient _httpClient;
    
    private const string BaseUrl = "https://api-pub.bitfinex.com/v2/";
    private const string TradesUrl = "trades/";
    private const string CandlesUrl = "candles/";

    public TestRestConnector(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    /// <summary>
    /// Реализация метода для конкретной биржи. Хорошим решением было бы внедрение ряда зависимостей и промежуточного абстрактного класса, но об этом в условии не сообщалось
    /// </summary>
    /// <exception cref="Exception">Люое возможное исключение "ломает" метод, в связи с чем считаю более логичным обрабатывать на верхних уровнях согласно бизнес логике и требованиям</exception>
    public async Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount, CancellationToken cancellationToken = default)
    {
        var endpointUrl = $"{BaseUrl}{TradesUrl}{pair}/hist?limit={maxCount}";
        using var result = await _httpClient.GetAsync(endpointUrl, cancellationToken);

        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"GetNewTradesAsync failed: {endpointUrl}");
        }

        try
        {
            var response = await result.Content.ReadAsStringAsync(cancellationToken);
            var data = JsonSerializer.Deserialize<List<object[]>>(response);
            var answer = new List<Trade>(data.Count);

            answer.AddRange(data.Select(tradeData => new Trade(pair, tradeData)));
            
            return answer;
        }
        catch (Exception ex)
        {
            throw new Exception($"GetNewTradesAsync failed: {endpointUrl}", ex);
        }
    }

    public async Task<IEnumerable<Candle>> GetCandleSeriesAsync(CandleSeriesQuery query, CancellationToken cancellationToken = default)
    {
        var endpointUrl = $"{BaseUrl}{CandlesUrl}{query.ToUrlParams()}";
        using var result = await _httpClient.GetAsync(endpointUrl, cancellationToken);

        if (!result.IsSuccessStatusCode)
        {
            throw new Exception($"GetCandleSeriesAsync failed: {endpointUrl}");
        }
        
        try
        {
            var response = await result.Content.ReadAsStringAsync(cancellationToken);
            var data = JsonSerializer.Deserialize<List<object[]>>(response);
            var answer = new List<Candle>(data.Count);

            answer.AddRange(data.Select(candleData => new Candle(query.Pair, candleData)));
            
            return answer;
        }
        catch (Exception ex)
        {
            throw new Exception($"GetCandleSeriesAsync failed: {endpointUrl}", ex);
        }
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}