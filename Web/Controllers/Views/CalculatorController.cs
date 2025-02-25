using Microsoft.AspNetCore.Mvc;
using Service.Entities;
using Service.Services;

namespace Web.Controllers.Views;

public class CalculatorController : Controller
{
    private readonly IConnectorService _connectorService;
    
    private static readonly List<Wallet> testTaskWallets = new List<Wallet>()
    {
        new Wallet("BTC", (decimal)0.05),
        new Wallet("XRP", (decimal)1.05),
        new Wallet("XMR", (decimal)2.05),
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