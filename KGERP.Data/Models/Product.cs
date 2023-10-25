//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace KGERP.Data.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Product
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Product()
        {
            this.ManagerProductMaps = new HashSet<ManagerProductMap>();
            this.IngredientStandards = new HashSet<IngredientStandard>();
            this.OrderDeliverDetails = new HashSet<OrderDeliverDetail>();
            this.PFormulaDetails = new HashSet<PFormulaDetail>();
            this.ProductDetails = new HashSet<ProductDetail>();
            this.ProductFormulas = new HashSet<ProductFormula>();
            this.ProductionDetails = new HashSet<ProductionDetail>();
            this.RequisitionItems = new HashSet<RequisitionItem>();
            this.SaleReturnDetails = new HashSet<SaleReturnDetail>();
            this.StoreDetails = new HashSet<StoreDetail>();
            this.OrderDetails = new HashSet<OrderDetail>();
            this.OrderDetailHistories = new HashSet<OrderDetailHistory>();
            this.DemageDetails = new HashSet<DemageDetail>();
        }
    
        public int ProductId { get; set; }
        public string ProductType { get; set; }
        public Nullable<int> CompanyId { get; set; }
        public Nullable<int> ProductCategoryId { get; set; }
        public Nullable<int> ProductSubCategoryId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ShortName { get; set; }
        public Nullable<decimal> UnitPrice { get; set; }
        public Nullable<decimal> CreditSalePrice { get; set; }
        public Nullable<decimal> SaleCommissionRate { get; set; }
        public Nullable<decimal> PurchaseRate { get; set; }
        public Nullable<decimal> PurchaseCommissionRate { get; set; }
        public decimal TPPrice { get; set; }
        public Nullable<int> UnitId { get; set; }
        public Nullable<double> Qty { get; set; }
        public Nullable<double> Consumption { get; set; }
        public Nullable<double> PackSize { get; set; }
        public Nullable<decimal> FormulaQty { get; set; }
        public Nullable<int> FacingId { get; set; }
        public string JsonData { get; set; }
        public decimal ProcessLoss { get; set; }
        public string Remarks { get; set; }
        public int OrderNo { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public Nullable<int> PackId { get; set; }
        public bool IsActive { get; set; }
        public string EngineNo { get; set; }
        public string ChassisNo { get; set; }
        public decimal CostingPrice { get; set; }
        public Nullable<decimal> DieSize { get; set; }
        public Nullable<int> AccountingHeadId { get; set; }
        public Nullable<int> AccountingIncomeHeadId { get; set; }
        public Nullable<int> AccountingExpenseHeadId { get; set; }
        public int Status { get; set; }
        public decimal VartualValue { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ManagerProductMap> ManagerProductMaps { get; set; }
        public virtual FacingInfo FacingInfo { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<IngredientStandard> IngredientStandards { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrderDeliverDetail> OrderDeliverDetails { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PFormulaDetail> PFormulaDetails { get; set; }
        public virtual ProductCategory ProductCategory { get; set; }
        public virtual ProductSubCategory ProductSubCategory { get; set; }
        public virtual Unit Unit { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductDetail> ProductDetails { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductFormula> ProductFormulas { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ProductionDetail> ProductionDetails { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RequisitionItem> RequisitionItems { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SaleReturnDetail> SaleReturnDetails { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StoreDetail> StoreDetails { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrderDetailHistory> OrderDetailHistories { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DemageDetail> DemageDetails { get; set; }
    }
}
