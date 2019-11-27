using Cassandra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateData
{
    class OrderDetails
    {
        public int orderid { get; set; }
        public int customerid { get; set; }
        public LocalDate orderdate { get; set; }
        public decimal ordervalue { get; set; }
    }
}
