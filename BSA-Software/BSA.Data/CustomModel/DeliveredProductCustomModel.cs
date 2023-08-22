using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSA.Data.CustomModel
{
   public class DeliveredProductCustomModel
    {
        public long OrderMasterId { get; set; }
        public int ProductId { get; set; }
        public string ProductBatchId { get; set; }
        public Nullable<double> OrderQty { get; set; }
        public Nullable<double> DeliveredQty { get; set; }
    }
}
