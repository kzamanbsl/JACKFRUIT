using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSA.Data.CustomModel
{
   public class ProductRequisitonFormullaModel
    {
        public int RProductId { get; set; }
        public string ProductName { get; set; }
        public decimal RMQ { get; set; }
        public decimal AvailableQty { get; set; }
    }
}
