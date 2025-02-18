using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Core.Entities
{
    public class HistoryResponse
    {
        public decimal amount { get; set; }
        public string @base { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
        public Dictionary<DateTime, Dictionary<string, decimal>> rates { get; set; }
    }
}
