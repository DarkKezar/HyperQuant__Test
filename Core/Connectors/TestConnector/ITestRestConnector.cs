using Core.DTO;
using Core.Models;

namespace Core.Connectors.TestConnector;

/// <summary>
/// Коннектор для REST запросов. По причине S из SOLID
/// </summary>
public interface ITestRestConnector : IDisposable
{
    /// <summary>
    /// Получение Ticker
    /// </summary>
    /// <param name="pair"></param>
    /// <param name="maxCount"></param>
    /// <param name="cancellationToken">Добален для корректной работы в ассинхронном режиме.</param>
    /// <returns></returns>
    Task<Ticker> GetTickerAsync(string pair, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получение сделок
    /// </summary>
    /// <param name="pair"></param>
    /// <param name="maxCount"></param>
    /// <param name="cancellationToken">Добален для корректной работы в ассинхронном режиме.</param>
    /// <returns></returns>
    Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount, CancellationToken cancellationToken  = default);
    
    /// <summary>
    /// Получение Candles
    /// </summary>
    /// <param name="query">Набор параметром "свернут", придерживаюсь правила - неболее 5 аргументов</param>
    /// <param name="cancellationToken">Добален для корректной работы в ассинхронном режиме.</param>
    /// <returns></returns>
    Task<IEnumerable<Candle>> GetCandleSeriesAsync(CandleSeriesQuery query, CancellationToken cancellationToken  = default);
}