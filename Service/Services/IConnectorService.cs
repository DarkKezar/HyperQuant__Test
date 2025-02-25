using Core.DTO;
using Core.Models;
using Service.Entities;

namespace Service.Services;

public interface IConnectorService : IDisposable
{
    Task<List<List<Wallet>>> ConvertPairAsync(List<Wallet> wallets);
    
    Task<Ticker> GetTickerAsync(string pair, CancellationToken cancellationToken = default);
    Task<IEnumerable<Trade>> GetTradesAsync(string pair, CancellationToken cancellationToken  = default);
    Task<IEnumerable<Candle>> GetCandleSeriesAsync(CandleSeriesQuery query, CancellationToken cancellationToken  = default);
    
    void AddNewBuyTrade(Action<Trade> newAction);
    void AddNewSellTrade(Action<Trade> newAction);
    void AddCandleSeriesProcessing(Action<Candle> newAction);
}