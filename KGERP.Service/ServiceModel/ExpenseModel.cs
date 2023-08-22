using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using KGERP.Service.Implementation.Configuration;
using KGERP.Utility;

namespace KGERP.Service.ServiceModel
{
    public class ExpenseModel : BaseVM
    {
        public int ExpenseMasterId { get; set; }
        public System.DateTime ExpenseDate { get; set; }
        public int PaymentMethod { get; set; }
        public int? TerritoryId { get; set; }
        public string TerritoryName { get; set; }
        public string Description { get; set; }
        public long? ExpenseBy { get; set; }
        public string ExpenseByName { get; set; }
        public string ExpenseNo { get; set; }

        [Required]
        public string ReferenceNo { get; set; }
        public int CompanyId { get; set; }

        public string CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public System.DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public int Status { get; set; }

        public int ExpensesId { get; set; }

        public decimal Amount { get; set; }

        public int? PaymentMasterId { get; set; }
        public int? ExpensesHeadGLId { get; set; }
        public string ExpensesHeadGLName { get; set; }

        public string StrFromDate { get; set; }
        public string StrToDate { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public IEnumerable<ExpenseModel> DataList { get; set; } = new List<ExpenseModel>();
        public IEnumerable<ExpenseDetailModel> DetailList { get; set; } = new List<ExpenseDetailModel>();

        public int ExpensePaymentMethodEnumFK { get; set; }
        public SelectList ExpensePaymentMethodList { get { return new SelectList(BaseFunctionalities.GetEnumList<VendorsPaymentMethodEnum>(), "Value", "Text"); } }
        public SelectList SubZoneList { get; set; } = new SelectList(new List<object>());
        // public SelectList ExpenseByList { get; set; } = new SelectList(new List<object>());

        public SelectList VoucherTypesList { get; set; } = new SelectList(new List<object>());
        public SelectList CostCenterList { get; set; } = new SelectList(new List<object>());
        public SelectList BankOrCashParentList { get; set; } = new SelectList(new List<object>());

        [Required]
        public int VoucherTypeId { get; set; }
        public int? Accounting_CostCenterFK { get; set; }
        public int Accounting_HeadFK { get; set; }
        [Required]
        public string AccountingHeadName { get; set; }
        public double Debit { get; set; } = 0;
        public double Credit { get; set; } = 0;
    }

    public class ExpenseDetailModel
    {
        public int ExpenseMasterId { get; set; }
        public int ExpensesId { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public decimal Amount { get; set; }
        public decimal? OutAmount { get; set; }
        public string ReferenceNo { get; set; }

        public string CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public System.DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public int? PaymentMasterId { get; set; }
        public string PaymentMasterName { get; set; }
        public int? ExpensesHeadGLId { get; set; }
        public string ExpensesHeadGLName { get; set; }


    }

}
