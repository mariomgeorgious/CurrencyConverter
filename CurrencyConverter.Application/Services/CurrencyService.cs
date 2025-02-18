using CurrencyConverter.Application.Factories;
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

namespace CurrencyConverter.Application.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly ICurrencyProviderFactory _providerFactory;

        public CurrencyService(ICurrencyProviderFactory providerFactory)
        {
            _providerFactory = providerFactory;
        }

        public async Task<ExchangeRateResponse> GetLatestRates(string providerName)
        {
            var provider = _providerFactory.GetProvider(providerName);
            return await provider.GetLatestRates();
        }

        public async Task<ExchangeRateResponse> ConvertCurrency(string providerName, string from, string to, decimal amount)
        {
            var provider = _providerFactory.GetProvider(providerName);
            return await provider.ConvertCurrency(from, to, amount);
        }

        public async Task<HistoryResponse> GetHistoricalRates(string providerName, string baseCurrency, DateTime start, DateTime end, int page, int size)
        {
            var provider = _providerFactory.GetProvider(providerName);
            return await provider.GetHistoricalRates(baseCurrency, start, end, page, size);
        }
    }
}
