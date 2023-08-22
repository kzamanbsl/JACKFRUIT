using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSA.Data.CustomModel
{
    public class DeliverItemCustomModel
    {

        //public long OrderDetailId { get; set; }
        //public long OrderDeliverId { get; set; }
        //public Nullable<long> OrderMasterId { get; set; }


        //public Nullable<int> ProductId { get; set; }
        //public string ProductName { get; set; }
        //public Nullable<double> OrderQty { get; set; }
        //public Nullable<double> OrderUnitPrice { get; set; }


        //public Nullable<double> AvailableQty { get; set; }
        //public Nullable<double> DeliveredQty { get; set; }
        //public Nullable<double> RemainingQty { get; set; }
        //public Nullable<double> DeliveryAmount { get; set; }

        //[DisplayName("Delivered Qty")]
        //public int DeliveredQty { get; set; }

        //[DisplayName("Due Qty")]
        //public int DueQty { get; set; }

        
        public int? ProductId { get; set; }
        public int StockInfoId { get; set; }


        [DisplayName("Product Code")]
        public string ProductCode { get; set; }
        [DisplayName("Product Name")]
        public string ProductName { get; set; }

        [DisplayName("Unit")]
        public string OrderUnit { get; set; }
        [DisplayName("Qty")]
        public Nullable<double> OrderQty { get; set; }
        [DisplayName("Unit Price")]
        public double? OrderUnitPrice { get; set; }
        [DisplayName("Delivered Qty")]
        public double? DeliveredQty { get; set; }
        [DisplayName("Due Qty")]
        public double? DueQty { get; set; }
        [DisplayName("Available Qty")]
        public double StoreAvailableQty { get; set; }
        [DisplayName("Ready To Deliver")]
        public double? ReadyToDeliver { get; set; }
        [DisplayName("Remaining Qty")]
        public double? OrderRemainingQty { get; set; }
        public decimal? Discount { get; set; }


        public decimal? SpecialBaseCommission { get; set; }
        
    }
}
