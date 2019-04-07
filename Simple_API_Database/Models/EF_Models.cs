// ADD THIS DIRECTIVES
using System.ComponentModel.DataAnnotations;

namespace Simple_API_Database.Models
{
    public class EF_Models
    {
        public class Company
        {
            [Key]
            public string symbol { get; set; }
            public string name { get; set; }
            public string date { get; set; }
            public bool isEnabled { get; set; }
            public string type { get; set; }
            public string iexId { get; set; }
        }

        public class News
        {
            [Key]
            public string datetime { get; set; }
            public string headline { get; set; }
            public string source { get; set; }
            public string summary { get; set; }
        }

        public class Quotes
        {
            [Key]
            public string symbol { get; set; }
            public string date { get; set; }
            public double open { get; set; }
            public double close { get; set; }
            public double high { get; set; }
            public double low { get; set; }
            public double changePercent { get; set; }

        }
    }
}
