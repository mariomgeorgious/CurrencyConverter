using CurrencyConverter.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Core.Interfaces
{
    public interface ICurrencyService
    {
        Task<ExchangeRateResponse> GetLatestRates(string provider);
        Task<ExchangeRateResponse> ConvertCurrency(string provider, string from, string to, decimal amount);
        Task<HistoryResponse> GetHistoricalRates(string provider, string baseCurrency, DateTime start, DateTime end, int page, int size);
    }
}
