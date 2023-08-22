using KGERP.Service.ServiceModel;
using KGERP.Utility;
using System.Collections.Generic;
using KGERP.Service.Implementation.Warehouse;

namespace KGERP.ViewModel
{
    public class MaterialReceiveViewModel
    {
        public MaterialReceiveModel MaterialReceive { get; set; }
        public List<MaterialReceiveDetailModel> MaterialReceiveDetails { get; set; }
        public List<SelectModel> Vendors { get; set; }
        public List<SelectModel> PurchaseOrders { get; set; }
        public List<SelectModel> StockInfos { get; set; }
        public List<SelectModel> BagWeights { get; set; }
        public VMWarehousePOReceivingSlave VMReceivingSlave { get; set; }
    }
}