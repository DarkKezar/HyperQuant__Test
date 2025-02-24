namespace Core.Models;

public class Trade
{
    private const string BUY = "buy";
    private const string SELL = "sell";
    
    /// <summary>
    /// Валютная пара
    /// </summary>
    public string Pair { get; set; }

    /// <summary>
    /// Цена трейда
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Объем трейда
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Направление (buy/sell)
    /// </summary>
    public string Side { get; set; }

    /// <summary>
    /// Время трейда
    /// </summary>
    public DateTimeOffset Time { get; set; }


    /// <summary>
    /// Id трейда
    /// </summary>
    public string Id { get; set; }
    
    
    public Trade(string pair, object[] values)
    {
        switch (values.Length)
        {
            case 4:
                Id = values[0].ToString();
                Time = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(values[1].ToString()));
                Amount = decimal.Parse(values[2].ToString());
                Price = decimal.Parse(values[3].ToString());
                Side = Amount > 0 ? BUY : SELL;
                break;
            case 5:
                Id = values[0].ToString();
                Time = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(values[1].ToString()));
                Amount = decimal.Parse(values[2].ToString());
                Price = decimal.Parse(values[3].ToString());
                Side = Amount > 0 ? BUY : SELL;
                break;
            default:
                throw new ArgumentException($"Unknown API results for {pair}");
        }
        Pair = pair;
    }
}