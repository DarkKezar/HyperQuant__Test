using Microsoft.AspNetCore.Mvc;
using Service.Entities;
using Service.Services;

namespace Web.Controllers.Views;

public class CalculatorController : Controller
{
    private readonly IConnectorService _connectorService;
    
    private static readonly List<Wallet> testTaskWallets = new List<Wallet>()
    {
        new Wallet("BTC", (decimal)1.000),
        new Wallet("XRP", (decimal)15000),
        new Wallet("XMR", (decimal)50.00),
        new Wallet("DSH", (decimal)30.00),
    };

    public CalculatorController(IConnectorService connectorService)
    {
        _connectorService = connectorService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var data = await _connectorService.ConvertPairAsync(testTaskWallets);
        return View(data);
    }
}