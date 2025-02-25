using System.Net;
using System.Text;
using Core.Connectors.TestConnector;
using Core.Constants;
using Core.DTO;
using Moq;
using Moq.Protected;

namespace xUnitTest.CoreTests;

public class TestRestConnectorTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _mockHttpClient;
    private readonly TestRestConnector _testRestConnector;

    public TestRestConnectorTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _mockHttpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _testRestConnector = new TestRestConnector(_mockHttpClient);
    }

    [Fact]
    public async Task GetTickerAsync_Success_ReturnsTicker()
    {
        var pair = "tBTCUSD";
        var expectedResponse = "[12345, 1.2345, 1.2346, 1.2347, 1.2348, 1.2349, 1.2350, 1.2351, 1.2352, 1.2353]";
        var endpointUrl = $"{BitfinexApiConstants.Urls.BaseUrl}{BitfinexApiConstants.Urls.Endpoints.Ticker}{pair}";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && 
                                                     req.RequestUri.ToString() == endpointUrl),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(expectedResponse, Encoding.UTF8, "application/json")
            });

        var result = await _testRestConnector.GetTickerAsync(pair);

        Assert.NotNull(result);
        Assert.Equal(pair, result.Pair);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetTickerAsync_Failure_ThrowsException()
    {
        var pair = "tBTCUSD";
        var endpointUrl = $"{BitfinexApiConstants.Urls.BaseUrl}{BitfinexApiConstants.Urls.Endpoints.Ticker}{pair}";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && 
                                                     req.RequestUri.ToString() == endpointUrl),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        await Assert.ThrowsAsync<Exception>(() => _testRestConnector.GetTickerAsync(pair));
    }

    [Fact]
    public async Task GetNewTradesAsync_Success_ReturnsTrades()
    {
        var pair = "tBTCUSD";
        var maxCount = 10;
        var expectedResponse = "[[1725232941,1740480501912,0.01501044,88277],[1725232939,1740480497493,-0.00744,88251],[1725232936,1740480495860,0.01199526,88315],[1725232935,1740480495858,0.0769,88289],[1725232934,1740480495857,0.0478,88289],[1725232933,1740480495856,0.15,88288],[1725232932,1740480495855,0.01777321,88281],[1725232931,1740480495854,0.00853776,88270],[1725232930,1740480495257,-0.0066,88251],[1725232929,1740480495138,-0.0777,88256]]";
        var endpointUrl = $"{BitfinexApiConstants.Urls.BaseUrl}{BitfinexApiConstants.Urls.Endpoints.Trades}{pair}/hist?limit={maxCount}";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && 
                                                     req.RequestUri.ToString() == endpointUrl),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(expectedResponse, Encoding.UTF8, "application/json")
            });

        var result = await _testRestConnector.GetNewTradesAsync(pair, maxCount);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.All(result, trade => Assert.Equal(pair, trade.Pair));
    }

    [Fact]
    public async Task GetNewTradesAsync_Failure_ThrowsException()
    {
        var pair = "tBTCUSD";
        var maxCount = 10;
        var endpointUrl = $"{BitfinexApiConstants.Urls.BaseUrl}{BitfinexApiConstants.Urls.Endpoints.Trades}{pair}/hist?limit={maxCount}";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && 
                                                     req.RequestUri.ToString() == endpointUrl),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        await Assert.ThrowsAsync<Exception>(() => _testRestConnector.GetNewTradesAsync(pair, maxCount));
    }

    [Fact]
    public async Task GetCandleSeriesAsync_Success_ReturnsCandles()
    {
        var query = new CandleSeriesQuery( "tBTCUSD", 60);
        var expectedResponse = "[[12345, 1.2345, 1.2346, 1.2347, 1.2348], [12346, 1.2349, 1.2350, 1.2351, 1.2352]]";
        var endpointUrl = $"{BitfinexApiConstants.Urls.BaseUrl}{BitfinexApiConstants.Urls.Endpoints.Candles}{query.ToUrlParams()}";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && 
                                                     req.RequestUri.ToString() == endpointUrl),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(expectedResponse, Encoding.UTF8, "application/json")
            });

        var result = await _testRestConnector.GetCandleSeriesAsync(query);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.All(result, candle => Assert.Equal(query.Pair, candle.Pair));
    }

    [Fact]
    public async Task GetCandleSeriesAsync_Failure_ThrowsException()
    {
        var query = new CandleSeriesQuery( "tBTCUSD", 60);
        var endpointUrl = $"{BitfinexApiConstants.Urls.BaseUrl}{BitfinexApiConstants.Urls.Endpoints.Candles}{query.ToUrlParams()}";

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && 
                                                     req.RequestUri.ToString() == endpointUrl),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest
            });

        await Assert.ThrowsAsync<Exception>(() => _testRestConnector.GetCandleSeriesAsync(query));
    }
}