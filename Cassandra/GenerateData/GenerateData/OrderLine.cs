using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateData
{
    class OrderLine
    {
        public int orderid { get; set; }
        public int orderline { get; set; }
        public string productname { get; set; }
        public short quantity { get; set; }
        public decimal orderlinecost { get; set; }
    }
}
