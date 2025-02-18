using CurrencyConverter.Application.Services;
using CurrencyConverter.Core.Entities;
using CurrencyConverter.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CurrencyConverter.Application.Providers
{
    public class FrankfurterProvider : ICurrencyProvider
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _config;
        private readonly ILogger<CurrencyService> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

        public FrankfurterProvider(HttpClient httpClient, IMemoryCache cache, IConfiguration config, ILogger<CurrencyService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _config = config;
            _logger = logger;

            _retryPolicy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, context) => _logger.LogWarning($"Retrying due to {exception.Message}"));

            _circuitBreakerPolicy = Policy
                .Handle<HttpRequestException>()
                .CircuitBreakerAsync(5, TimeSpan.FromMinutes(1),
                    onBreak: (ex, time) => _logger.LogError($"Circuit broken: {ex.Message}"),
                    onReset: () => _logger.LogInformation("Circuit reset"),
                    onHalfOpen: () => _logger.LogInformation("Circuit in half-open state"));
        }

        public async Task<ExchangeRateResponse> GetLatestRates()
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                return await _circuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    string cacheKey = $"latest_rates";
                    if (_cache.TryGetValue(cacheKey, out ExchangeRateResponse cachedResponse))
                        return cachedResponse;
                    
                    var response = await _httpClient.GetAsync($"{_config["ExchangeRateApi:BaseUrl"]}{_config["ExchangeRateApi:LatestRatesUrl"]}");
                    if (!response.IsSuccessStatusCode) return null;

                    var result = JsonSerializer.Deserialize<ExchangeRateResponse>(await response.Content.ReadAsStringAsync());
                    _cache.Set(cacheKey, result, TimeSpan.FromMinutes(30));
                    return result;
                });
            });
        }

        public async Task<ExchangeRateResponse> ConvertCurrency(string from, string to, decimal amount)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                return await _circuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    var response = await _httpClient.GetAsync($"{_config["ExchangeRateApi:BaseUrl"]}{string.Format(_config["ExchangeRateApi:ConvertCurrencyUrl"], from, to, amount)}");
                    if (!response.IsSuccessStatusCode) return null;

                    return JsonSerializer.Deserialize<ExchangeRateResponse>(await response.Content.ReadAsStringAsync());
                });
            });
        }

        public async Task<HistoryResponse> GetHistoricalRates(string baseCurrency, DateTime start, DateTime end, int page, int size)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                return await _circuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    var strStart = $"{start:yyyy-MM-dd}";
                    var strEnd = $"{end:yyyy-MM-dd}";

                    var response = await _httpClient.GetAsync($"{_config["ExchangeRateApi:BaseUrl"]}{string.Format(_config["ExchangeRateApi:GetHistoricalRatesUrl"], strStart, strEnd, baseCurrency, page, size)}");

                    if (!response.IsSuccessStatusCode) return null;

                    var result = JsonSerializer.Deserialize<HistoryResponse>(await response.Content.ReadAsStringAsync());
                    return result;
                });
            });
        }
    }
}
