using CurrencyConverter.Application.Providers;
using CurrencyConverter.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Application.Factories
{
    public class CurrencyProviderFactory : ICurrencyProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public CurrencyProviderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ICurrencyProvider GetProvider(string providerName)
        {
            switch (providerName.ToLower())
            {
                case "frankfurter":
                    return _serviceProvider.GetRequiredService<FrankfurterProvider>();
                default:
                    throw new NotImplementedException($"Provider {providerName} not implemented.");
            }
        }
    }
}
