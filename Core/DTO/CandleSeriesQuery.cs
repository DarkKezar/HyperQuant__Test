namespace Core.DTO;

public class CandleSeriesQuery
{
    /// <summary>
    /// Список допустимых значений из документации API
    /// </summary>
    private static readonly Dictionary<string, int> AllowedTimeFrame = new()
    {
        { "1m", 60 },
        { "5m", 300 },
        { "15m", 900 },
        { "30m", 1800 },
        { "1h", 3600 },
        { "3h", 10800 },
        { "6h", 21600 },
        { "12h", 43200 },
        { "1D", 86400 },
        { "1W", 604800 },
        { "14D", 1209600 },
        { "1M", 2592000 } 
    };
    
    /// <summary>
    /// Выборка наиболее близкого значения по кол-ву секунд
    /// </summary>
    /// <param name="seconds">Кол-во секунд указанное пользователем, не обязано совпадать с списком из API</param>
    /// <returns>Возвращает ближайший по времени эквивалент из заданного списка</returns>
    public static string ConvertSecondsToAllowedTimeFrame(int seconds)
    {
        var nearestInterval = AllowedTimeFrame
            .OrderBy(interval => Math.Abs(seconds - interval.Value)) 
            .First() 
            .Key; 

        return nearestInterval;
    }
    
    public string Pair { get; set; } 
    public int PeriodInSec { get; set; } 
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; } = null;
    public long? Count { get; set; } = 0;

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="pair">Название - единственный параметр без которого не может существовать данный запрос</param>
    /// <param name="periodInSec">Опциональный параметр (период)</param>
    /// <param name="from">Опциональный параметр (Начала периода)</param>
    /// <param name="to">Опциональный параметр (Конец периода)</param>
    /// <param name="count">Опциональный параметр (Кол-во)</param>
    public CandleSeriesQuery(string pair, int periodInSec = 0, DateTimeOffset? from = null, DateTimeOffset? to = null,
        long? count = 0)
    {
        Pair = pair;
        PeriodInSec = periodInSec;
        From = from;
        To = to;
        Count = count;
    }

    /// <summary>
    /// Возвращает аргументы URL запроса, чтобы упростить логику формирования внутри коннектора
    /// </summary>
    /// <returns>Строка содержащая хранимые параметры в нужном формате</returns>
    public string ToUrlParams()
    {
        var urlParts = new List<string>
        {
            $"trade:{ConvertSecondsToAllowedTimeFrame(PeriodInSec) ?? "1m"}:{Pair ?? string.Empty}/hist"
        };

        var queryParams = new List<string>();

        if (From.HasValue)
        {
            queryParams.Add($"start={From?.ToUnixTimeMilliseconds()}");
        }

        if (To.HasValue)
        {
            queryParams.Add($"end={To?.ToUnixTimeMilliseconds()}");
        }

        if (Count > 0)
        {
            queryParams.Add($"limit={Count}");
        }

        if (queryParams.Any())
        {
            urlParts.Add($"?{string.Join("&", queryParams)}");
        }

        return string.Join("", urlParts);
    }
}