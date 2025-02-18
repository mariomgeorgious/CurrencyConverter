using CurrencyConverter.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Core.Interfaces
{
    public interface ICurrencyProvider
    {
        Task<ExchangeRateResponse> GetLatestRates();
        Task<ExchangeRateResponse> ConvertCurrency(string from, string to, decimal amount);
        Task<HistoryResponse> GetHistoricalRates(string baseCurrency, DateTime start, DateTime end, int page, int size);
    }
}
