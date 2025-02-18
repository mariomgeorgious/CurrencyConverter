using CurrencyConverter.Core.Entities;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Application.Validators
{
    public class HistoricalRatesRequestValidator : AbstractValidator<HistoricalRatesRequest>
    {
        public HistoricalRatesRequestValidator()
        {
            RuleFor(x => x.BaseCurrency)
                .NotEmpty().WithMessage("Base currency is required.")
                .Length(3).WithMessage("Currency code must be exactly 3 characters.");

            RuleFor(x => x.Start)
                .LessThanOrEqualTo(x => x.End)
                .WithMessage("Start date must be before or equal to the end date.");

            RuleFor(x => x.Page)
                .GreaterThan(0).WithMessage("Page must be greater than zero.");

            RuleFor(x => x.Size)
                .GreaterThan(0).WithMessage("Size must be greater than zero.");
        }
    }
}
