using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Core.Entities
{
    public class HistoricalRatesRequest
    {
        public string BaseCurrency { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
    }
}
