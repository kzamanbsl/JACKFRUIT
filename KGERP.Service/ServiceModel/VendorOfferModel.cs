using System;
using System.ComponentModel;

namespace KGERP.Service.ServiceModel
{
    public class VendorOfferModel
    {
        public string ButtonName
        {
            get
            {
                return VendorOfferId > 0 ? "Update" : "Save";
            }
        }
        public long VendorOfferId { get; set; }
        public int VendorId { get; set; }
        [DisplayName("Product")]
        public int ProductId { get; set; }
        public string ProductType { get; set; }
        [DisplayName("Base")]
        public Nullable<decimal> BaseCommission { get; set; }
        [DisplayName("Cash")]
        public Nullable<decimal> CashCommission { get; set; }
        [DisplayName("Carrying")]
        public Nullable<decimal> CarryingCommission { get; set; }
        [DisplayName("FactoryCarrying")]
        public Nullable<decimal> FactoryCarryingCommission { get; set; }
        [DisplayName("Special")]
        public Nullable<decimal> SpecialCommission { get; set; }
        [DisplayName("Addition Price")]
        public Nullable<decimal> AdditionPrice { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        [DisplayName("Active")]
        public bool IsActive { get; set; }

        public virtual ProductModel Product { get; set; }
        public virtual VendorModel Vendor { get; set; }

        //-----------Extended Properties---------------

        public string ProductCategory { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string CustomerName { get; set; }
        public bool IsAllBase { get; set; }
        public bool IsAllCash { get; set; }
        public bool IsAllCarrying { get; set; }
        public bool IsAllFactoryCarrying { get; set; }
        public bool IsAllSpecial { get; set; }
        public bool IsAllAdditionPrice { get; set; }
    }
}
