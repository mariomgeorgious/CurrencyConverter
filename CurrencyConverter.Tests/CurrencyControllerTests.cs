using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using CurrencyConverter.Api.Controllers;
using CurrencyConverter.Application.Services;
using CurrencyConverter.Core.Entities;
using CurrencyConverter.Core.Interfaces;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CurrencyConverter.Tests.Controllers
{
    public class CurrencyControllerTests
    {
        private readonly Mock<ICurrencyService> _mockCurrencyService;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<ILogger<CurrencyController>> _mockLogger;
        private readonly CurrencyController _controller;

        public CurrencyControllerTests()
        {
            _mockCurrencyService = new Mock<ICurrencyService>();
            _mockConfig = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<CurrencyController>>();

            _mockConfig.Setup(c => c["CurrentProvider"]).Returns("Frankfurter");
            _controller = new CurrencyController(_mockCurrencyService.Object, _mockConfig.Object, _mockLogger.Object);
        }

        #region GetLatestRates
        [Fact]
        public async Task GetLatestRates_ReturnsOk_WithValidData()
        {
            var expectedRates = new Dictionary<string, decimal> { { "USD", 1.2m }, { "EUR", 1.0m } };
            var expectedResponse = new ExchangeRateResponse { rates = expectedRates };
            _mockCurrencyService.Setup(s => s.GetLatestRates("Frankfurter")).ReturnsAsync(expectedResponse);

            var result = await _controller.GetLatestRates();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResponse, okResult.Value);
        }

        [Fact]
        public async Task GetLatestRates_ReturnsNotFound_WhenNoData()
        {
            _mockCurrencyService.Setup(s => s.GetLatestRates("Frankfurter")).ReturnsAsync((ExchangeRateResponse)null);

            var result = await _controller.GetLatestRates();

            Assert.IsType<NotFoundResult>(result);
        }
        #endregion

        #region ConvertCurrency
        [Fact]
        public async Task ConvertCurrency_ReturnsOk_WithValidData()
        {
            var request = new CurrencyConversionRequest { FromCurrency = "USD", ToCurrency = "EUR", Amount = 100 };

            var expectedRates = new Dictionary<string, decimal> { { "USD", 1.2m }, { "EUR", 1.0m } };
            var expectedResponse = new ExchangeRateResponse { rates = expectedRates };

            _mockCurrencyService.Setup(s => s.ConvertCurrency("Frankfurter", "USD", "EUR", 100)).ReturnsAsync(expectedResponse);

            var result = await _controller.ConvertCurrency(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResponse, okResult.Value);
        }

        [Theory]
        [InlineData("TRY", "USD")]
        [InlineData("USD", "PLN")]
        [InlineData("THB", "THB")]
        [InlineData("MXN", "EUR")]
        public async Task ConvertCurrency_ReturnsBadRequest_ForRestrictedCurrencies(string from, string to)
        {
            var request = new CurrencyConversionRequest { FromCurrency = from, ToCurrency = to, Amount = 100 };

            var result = await _controller.ConvertCurrency(request);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Conversion for specified currencies is not allowed.", badRequestResult.Value);
        }

        [Fact]
        public async Task ConvertCurrency_ReturnsBadRequest_ForNegativeAmount()
        {
            var request = new CurrencyConversionRequest { FromCurrency = "USD", ToCurrency = "EUR", Amount = -10 };

            var result = await _controller.ConvertCurrency(request);

            var badRequestResult = Assert.IsType<BadRequestResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }
        #endregion

        #region GetHistoricalRates
        [Fact]
        public async Task GetHistoricalRates_ReturnsOk_WithValidData()
        {
            var request = new HistoricalRatesRequest { BaseCurrency = "EUR", Start = DateTime.Today.AddDays(-30), End = DateTime.Today, Page = 1, Size = 10 };

            var expectedHistory = new HistoryResponse
            {
                amount = 100,
                @base = "USD",
                start_date = DateTime.Today.AddDays(-30),
                end_date = DateTime.Today,
                rates = new Dictionary<DateTime, Dictionary<string, decimal>>
                {
                    { DateTime.Today, new Dictionary<string, decimal> { { "EUR", 0.85m } } }
                }
            };
            _mockCurrencyService.Setup(s => s.GetHistoricalRates("Frankfurter", "EUR", It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1, 10)).ReturnsAsync(expectedHistory);

            var result = await _controller.GetHistoricalRates(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedHistory, okResult.Value);
        }

        [Fact]
        public async Task GetHistoricalRates_ReturnsNotFound_WhenNoData()
        {
            var request = new HistoricalRatesRequest { BaseCurrency = "EUR", Start = DateTime.Today.AddDays(-30), End = DateTime.Today, Page = 1, Size = 10 };

            _mockCurrencyService.Setup(s => s.GetHistoricalRates("Frankfurter", "USD", It.IsAny<DateTime>(), It.IsAny<DateTime>(), 1, 10)).ReturnsAsync((HistoryResponse)null);

            var result = await _controller.GetHistoricalRates(request);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetHistoricalRates_ReturnsBadRequest_ForInvalidDateRange()
        {
            var request = new HistoricalRatesRequest { BaseCurrency = "EUR", Start = DateTime.Today.AddDays(30), End = DateTime.Today, Page = 1, Size = 10 };

            var result = await _controller.GetHistoricalRates(request);

            var badRequestResult = Assert.IsType<BadRequestResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task GetHistoricalRates_ReturnsBadRequest_WhenBaseCurrencyIsEmpty()
        {
            var request = new HistoricalRatesRequest
            {
                BaseCurrency = string.Empty,
                Start = DateTime.Today.AddDays(-30),
                End = DateTime.Today,
                Page = 1,
                Size = 10
            };

            var result = await _controller.GetHistoricalRates(request);

            var badRequestResult = Assert.IsType<BadRequestResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task GetHistoricalRates_ReturnsBadRequest_WhenBaseCurrencyLessThan3characters()
        {
            var request = new HistoricalRatesRequest
            {
                BaseCurrency = "EU",
                Start = DateTime.Today.AddDays(-30),
                End = DateTime.Today,
                Page = 1,
                Size = 10
            };

            var result = await _controller.GetHistoricalRates(request);

            var badRequestResult = Assert.IsType<BadRequestResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task GetHistoricalRates_ReturnsBadRequest_WhenPageNotGreaterThanZero()
        {
            var request = new HistoricalRatesRequest
            {
                BaseCurrency = "EUR",
                Start = DateTime.Today.AddDays(-30),
                End = DateTime.Today,
                Page = 0,
                Size = 10
            };

            var result = await _controller.GetHistoricalRates(request);

            var badRequestResult = Assert.IsType<BadRequestResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task GetHistoricalRates_ReturnsBadRequest_WhenSizeNotGreaterThanZero()
        {
            var request = new HistoricalRatesRequest
            {
                BaseCurrency = "EUR",
                Start = DateTime.Today.AddDays(-30),
                End = DateTime.Today,
                Page = 1,
                Size = 0
            };

            var result = await _controller.GetHistoricalRates(request);

            var badRequestResult = Assert.IsType<BadRequestResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }
        #endregion
    }
}
