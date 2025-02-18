using CurrencyConverter.Application.Services;
using CurrencyConverter.Core.Entities;
using CurrencyConverter.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/Currency")]
    [ApiVersion("1.0")]
    public class CurrencyController : Controller
    {
        private readonly ICurrencyService _currencyService;
        private IConfiguration _config;
        private readonly ILogger<CurrencyController> _logger;

        public CurrencyController(ICurrencyService currencyService, IConfiguration config, ILogger<CurrencyController> logger)
        {
            _currencyService = currencyService;
            _config = config;
            _logger = logger;
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestRates()
        {
            _logger.LogInformation("Fetching latest rates");

            var result = await _currencyService.GetLatestRates(_config["CurrentProvider"]);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpPost("convert")]
        public async Task<IActionResult> ConvertCurrency([FromBody] CurrencyConversionRequest request)
        {
            _logger.LogInformation("Converting {Amount} from {FromCurrency} to {ToCurrency}", request.Amount, request.FromCurrency, request.ToCurrency);

            if (request.FromCurrency is "TRY" or "PLN" or "THB" or "MXN" || request.ToCurrency is "TRY" or "PLN" or "THB" or "MXN")
                return BadRequest("Conversion for specified currencies is not allowed.");

            var result = await _currencyService.ConvertCurrency(_config["CurrentProvider"], request.FromCurrency, request.ToCurrency, request.Amount);
            return result != null ? Ok(result) : BadRequest();
        }

        [HttpPost("history")]
        public async Task<IActionResult> GetHistoricalRates([FromBody] HistoricalRatesRequest request)
        {
            _logger.LogInformation("Fetching historical rates for {BaseCurrency} from {Start} to {End}", request.BaseCurrency, request.Start, request.End);

            var result = await _currencyService.GetHistoricalRates(_config["CurrentProvider"], request.BaseCurrency, request.Start, request.End, request.Page, request.Size);


            return result != null ? (result.rates.Any() ? Ok(result) : NotFound()) : BadRequest();
        }
    }
}
