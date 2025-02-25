using Core.Connectors.TestConnector;
using Core.Constants;
using Core.DTO;
using Core.Models;
using Service.Entities;

namespace Service.Services;

public class ConnectorService : IConnectorService
{
    private readonly ITestRestConnector _restConnector;
    private readonly ITestWSConnector _wSConnector;

    public ConnectorService(ITestRestConnector restConnector, ITestWSConnector wSConnector)
    {
        _restConnector = restConnector;
        _wSConnector = wSConnector;
    }
    
    public async Task<List<List<Wallet>>> ConvertPairAsync(List<Wallet> wallets)
    {
        if (wallets == null || wallets.Count < 2)
        {
            throw new ArgumentException("Invalid wallets");
        } 
        
        var currencies = new HashSet<string>();

        foreach (var wallet in wallets)
        {
            currencies.Add(wallet.Currency);
        }
        
        var prices = new Dictionary<string, decimal>(currencies.Count);

        foreach (var currency in currencies)
        {
            var course = await _restConnector.GetTickerAsync("t" + currency + BitfinexApiConstants.Symbols.Usd);
            prices.Add(currency, course.LastPrice);
        }
        
        var ans = new List<List<Wallet>>(wallets.Count);

        foreach (var wallet in wallets)
        {
            var usd = wallet.Amount * prices[wallet.Currency];
            var raw = new List<Wallet>(wallets.Count + 1)
            {
                new Wallet(BitfinexApiConstants.Symbols.Usd, usd),
                wallet
            };
            
            foreach (var currency in currencies)
            {
                if (currency != wallet.Currency)
                {
                    raw.Add(new Wallet(currency, usd / prices[currency]));
                }
            }
            
            raw.Sort((x, y) => x.Currency.CompareTo(y.Currency));
            ans.Add(raw);
        }

        return ans;
    }

    public async Task<Ticker> GetTickerAsync(string pair, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Trade>> GetTradesAsync(string pair, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Candle>> GetCandleSeriesAsync(CandleSeriesQuery query, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void AddNewBuyTrade(Action<Trade> newAction)
    {
        _wSConnector.NewBuyTrade += newAction;
    }

    public void AddNewSellTrade(Action<Trade> newAction)
    {
        _wSConnector.NewSellTrade += newAction;

    }

    public void AddCandleSeriesProcessing(Action<Candle> newAction)
    {
        _wSConnector.CandleSeriesProcessing += newAction;
    }
    
    public void Dispose()
    {
        _restConnector.Dispose();
        _wSConnector.Dispose();
    }
}