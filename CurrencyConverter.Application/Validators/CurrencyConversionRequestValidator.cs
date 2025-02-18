using CurrencyConverter.Core.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Application.Validators
{
    public class CurrencyConversionRequestValidator : AbstractValidator<CurrencyConversionRequest>
    {
        public CurrencyConversionRequestValidator()
        {
            RuleFor(x => x.FromCurrency)
                .NotEmpty().WithMessage("From currency is required.")
                .Length(3).WithMessage("Currency code must be exactly 3 characters.");

            RuleFor(x => x.ToCurrency)
                .NotEmpty().WithMessage("To currency is required.")
                .Length(3).WithMessage("Currency code must be exactly 3 characters.")
                .NotEqual(x => x.FromCurrency).WithMessage("From and To currencies cannot be the same.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than zero.");

            RuleFor(x => x)
                .Must(x => !IsRestrictedCurrency(x.FromCurrency) && !IsRestrictedCurrency(x.ToCurrency))
                .WithMessage("Conversion for specified currencies is not allowed.");
        }

        private bool IsRestrictedCurrency(string currency)
        {
            var restrictedCurrencies = new HashSet<string> { "TRY", "PLN", "THB", "MXN" };
            return restrictedCurrencies.Contains(currency?.ToUpper());
        }
    }
}
