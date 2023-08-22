using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System.Collections.Generic;

namespace KGERP.ViewModel
{
    public class PurchaseOrderViewModel
    {
        public PurchaseOrderModel PurchaseOrder { get; set; }
        public List<SelectModel> Demands { get; set; }
        public List<SelectModel> RawMaterials { get; set; }
        public List<SelectModel> Countries { get; set; }
        public List<SelectModel> ModeOfPurchases { get; set; }
        public List<SelectModel> ProductOrigins { get; set; }
        public PurchaseOrderDetailModel PurchaseOrderDetail { get; set; }
    }
}