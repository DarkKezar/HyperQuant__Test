namespace Core.Models;

public class Ticker
{
    /// <summary>
    /// Name
    /// </summary>
    public string Pair { get; set; }
    /// <summary>
    /// Price of last highest bid
    /// </summary>
    public decimal Bid { get; set; }
    /// <summary>
    /// Sum of the 25 highest bid sizes
    /// </summary>
    public decimal BidSize { get; set; }
    /// <summary>
    /// Price of last lowest ask
    /// </summary>
    public decimal Ask { get; set; }
    /// <summary>
    /// Sum of the 25 lowest ask sizes
    /// </summary>
    public decimal AskSize { get; set; }
    /// <summary>
    /// Amount that the last price has changed since yesterday
    /// </summary>
    public decimal DailyChange { get; set; }
    /// <summary>
    /// Relative price change since yesterday (*100 for percentage change)
    /// </summary>
    public decimal DailyChangeRelative { get; set; }
    /// <summary>
    /// Price of the last trade
    /// </summary>
    public decimal LastPrice { get; set; }
    /// <summary>
    /// Daily volume
    /// </summary>
    public decimal Volume { get; set; }
    /// <summary>
    /// Daily high
    /// </summary>
    public decimal High { get; set; }
    /// <summary>
    /// Daily low
    /// </summary>
    public decimal Low { get; set; }

    public Ticker(string pair, object[] values)
    {
        Pair = pair;
        if (values != null && values.Length == 10)
        {
            Bid = Convert.ToDecimal(values[0].ToString());
            BidSize = Convert.ToDecimal(values[1].ToString());
            Ask = Convert.ToDecimal(values[2].ToString());
            AskSize = Convert.ToDecimal(values[3].ToString());
            DailyChange = Convert.ToDecimal(values[4].ToString());
            DailyChangeRelative = Convert.ToDecimal(values[5].ToString());
            LastPrice = Convert.ToDecimal(values[6].ToString());
            Volume = Convert.ToDecimal(values[7].ToString());
            High = Convert.ToDecimal(values[8].ToString());
            Low = Convert.ToDecimal(values[9].ToString());
        }
        else
        {
            throw new ArgumentException($"Invalid arguments for ticker '{pair}'");
        }
    }
}