using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSA.Data.CustomModel
{
  public  class OrderDeliverCustomModel
    {
        public long OrderMasterId { get; set; }
        [DisplayName("Order No")]
        public string OrderNo { get; set; }
        [DisplayName("Order Date")]
        public DateTime? OrderDate { get; set; }
        [DisplayName("Customer")]
        public string Customer { get; set; }
        [DisplayName("Address")]
        public string CustomerAddress { get; set; }
        [DisplayName("Contact No")]
        public string CustomerContact { get; set; }
        [DisplayName("Delivery Date")]
        public DateTime DeliveryDate { get; set; }
        [Required(ErrorMessage ="Challen No is required !")]
        [DisplayName("Challan No")]
        public string ChallanNo { get; set; }
        [DisplayName("Bill No")]
        public string InvoiceNo { get; set; }
        [DisplayName("Vehicle No")]
        [Required(ErrorMessage = "Vehicle Information is required !")]
        public string VehicleNo { get; set; }
        [DisplayName("Driver Name")]
        public string DriverName { get; set; }
        [DisplayName("Store")]
        [Required(ErrorMessage = "Please select a Store !")]
        public int StockInfoId { get; set; }
    }
}
