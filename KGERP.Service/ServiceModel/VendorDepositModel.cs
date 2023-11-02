using KGERP.Service.Implementation.KGRE;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace KGERP.Service.ServiceModel
{
    public class VendorDepositModel : BaseVM
    {

        public int VendorDepositId { get; set; }
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public Nullable<long> VoucherId { get; set; }
        public string VoucherTypeName { get; set; }
        public int VendorTypeId { get; set; }
        public string VendorTypeName { get; set; }
        public System.DateTime DepositDate { get; set; }
        public decimal DepositAmount { get; set; }
        public string Description { get; set; }
        public string BankChargeHeadGLName { get; set; }
        public string PaymentToHeadGLName { get; set; }
        public decimal BankCharge { get; set; }
        public int? Accounting_BankOrCashParantId { get; set; }
        public int? Accounting_BankOrCashId { get; set; }

        public bool IsActive { get; set; }
        public bool IsSubmit { get; set; }
        public System.DateTime CreateDate { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string ModifiedBy { get; set; }
        public int? CompanyId { get; set; }
        public SelectList BankOrCashParantList { get; set; } = new SelectList(new List<object>());
        public SelectList BankOrCashGLList { get; set; } = new SelectList(new List<object>());
        public List<VendorDepositModel> DataList { get; set; } = new List<VendorDepositModel>();
        public SelectList DeportList { get; set; } = new SelectList(new List<object>());
        public SelectList DealerList { get; set; } = new SelectList(new List<object>());
        public SelectList CustomerList { get; set; } = new SelectList(new List<object>());

    }
}
