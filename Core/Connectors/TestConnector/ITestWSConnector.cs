using Core.DTO;
using Core.Models;

namespace Core.Connectors.TestConnector;

/// <summary>
/// Коннектор для WS обновлений. По причине S из SOLID
/// </summary>
public interface ITestWSConnector : IDisposable
{
    /// <summary>
    /// NewBuyTrade может быть объединен с NewSellTrade.
    /// </summary>
    event Action<Trade> NewBuyTrade;
    /// <summary>
    /// NewSellTrade может быть объединен с NewBuyTrade.
    /// </summary>
    event Action<Trade> NewSellTrade;
    event Action<Candle> CandleSeriesProcessing;
    
    /// <summary>
    /// Подписывание на WS событие
    /// </summary>
    /// <param name="pair">Наименование валютной пары</param>
    /// <param name="maxCount">Максимальное число</param>
    /// <param name="cancellationToken">Добален для корректной работы в ассинхронном режиме.</param>
    void SubscribeTrades(string pair, int maxCount = 100, CancellationToken cancellationToken  = default);
    /// <summary>
    /// Отписывание от WS событие
    /// </summary>
    /// <param name="pair">Наименование валютной пары</param>
    /// <param name="maxCount">Максимальное число</param>
    /// <param name="cancellationToken">Добален для корректной работы в ассинхронном режиме.</param>
    void UnsubscribeTrades(string pair, CancellationToken cancellationToken  = default);
    /// <summary>
    /// Подписывание на WS событие
    /// </summary>
    /// <param name="pair">Наименование валютной пары</param>
    /// <param name="maxCount">Максимальное число</param>
    /// <param name="cancellationToken">Добален для корректной работы в ассинхронном режиме.</param>
    void SubscribeCandles(CandleSeriesQuery query, CancellationToken cancellationToken  = default);
    /// <summary>
    /// Отписывание от WS событие
    /// </summary>
    /// <param name="pair">Наименование валютной пары</param>
    /// <param name="maxCount">Максимальное число</param>
    /// <param name="cancellationToken">Добален для корректной работы в ассинхронном режиме.</param>
    void UnsubscribeCandles(string pair, CancellationToken cancellationToken  = default);
}