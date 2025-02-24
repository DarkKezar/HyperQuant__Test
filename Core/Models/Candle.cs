namespace Core.Models;

public class Candle
{
    /// <summary>
    /// Валютная пара
    /// </summary>
    public string Pair { get; set; }

    /// <summary>
    /// Цена открытия
    /// </summary>
    public decimal OpenPrice { get; set; }

    /// <summary>
    /// Максимальная цена
    /// </summary>
    public decimal HighPrice { get; set; }

    /// <summary>
    /// Минимальная цена
    /// </summary>
    public decimal LowPrice { get; set; }

    /// <summary>
    /// Цена закрытия
    /// </summary>
    public decimal ClosePrice { get; set; }


    /// <summary>
    /// Partial (Общая сумма сделок)
    /// </summary>
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// Partial (Общий объем)
    /// </summary>
    public decimal TotalVolume { get; set; }

    /// <summary>
    /// Время
    /// </summary>
    public DateTimeOffset OpenTime { get; set; }

    public Candle(string pair, object[] values)
    {
        if (values.Length == 6)
        {
            OpenTime = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(values[0].ToString()) / 1000);

            OpenPrice = Convert.ToDecimal(values[1].ToString());
            ClosePrice = Convert.ToDecimal(values[2].ToString());
            HighPrice = Convert.ToDecimal(values[3].ToString());
            LowPrice = Convert.ToDecimal(values[4].ToString());
            TotalVolume = (decimal)Convert.ToDouble(values[5].ToString());
            
            //By Formula?
            //TotalPrice = Convert.ToDecimal(values[4]);
        }
        Pair = pair;
    }
}