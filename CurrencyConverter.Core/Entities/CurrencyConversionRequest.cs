using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Core.Entities
{
    public class CurrencyConversionRequest
    {
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal Amount { get; set; }
    }
}
