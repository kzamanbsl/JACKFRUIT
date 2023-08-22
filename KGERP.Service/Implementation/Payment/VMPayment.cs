using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using KGERP.Service.Implementation.Configuration;


namespace KGERP.Service.Implementation
{
    public partial class VMPayment: VMPaymentMaster
    {

        public string ACName { get; set; }
        public string ACNo { get; set; }
        public string BankName { get; set; }
      // public int BranchName { get; set; }

        public int PaymentId { get; set; }
        public decimal InAmount { get; set; }
        public decimal? OutAmount { get; set; }
        public string DepositType { get; set; }
        public string OrderNo { get; set; }
        public DateTime OrderDate { get; set; }

        public long? OrderMasterId { get; set; }
        public long? PurchaseOrderId { get; set; }
        public string CommonCustomerName { get; set; }
        public string CommonCustomerCode { get; set; }
        public double PayableAmount { get; set; }
        public decimal PayableAmountDecimal { get; set; }

        public decimal ReturnAmount { get; set; }
        public int? CustomerId { get; set; }
        public int? SubZoneFk { get; set; }

        public int? ExpensesHeadGLId { get; set; }
        public decimal? ExpensesAmount { get; set; }
        public string ExpensesHeadGLName { get; set; }

        public int? OthersIncomeHeadGLId { get; set; }
        public decimal? OthersIncomeAmount { get; set; }
        public string OthersIncomeHeadGLName { get; set; }

        public string PaymentFromHeadGLName { get; set; }

        public int? Accounting_BankOrCashParantId { get; set; }
        public int? Accounting_BankOrCashId { get; set; }
       public IEnumerable<VMPayment> DataList { get; set; }
        public IEnumerable<VMPayment> DataListExpenses { get; set; }
        public IEnumerable<VMPayment> DataListIncome { get; set; }

        public SelectList OrderMusterList { get; set; } = new SelectList(new List<object>());
        public SelectList BankOrCashParantList { get; set; } = new SelectList(new List<object>());
        public SelectList ExpensesHeadList { get; set; } = new SelectList(new List<object>());
        public SelectList IncomeHeadList { get; set; } = new SelectList(new List<object>());

        public SelectList BankOrCashGLList { get; set; } = new SelectList(new List<object>());
        public SelectList SubZoneList { get; set; } = new SelectList(new List<object>());
        public SelectList CustomerList { get; set; } = new SelectList(new List<object>());


    }

    public partial class VMPaymentMaster: BaseVM
    {
        public int PaymentMasterId { get; set; }
        public int CompanyId { get; set; }
        public string ProductType { get; set; }
        public string StringDate { get; set; }
        public int VendorId { get; set; }
        public Nullable<int> PaymentModeId { get; set; }
        public string ReferenceNo { get; set; }
        public string ExpensessReference { get; set; }
        public string IncomeReference { get; set; }
        public System.DateTime TransactionDate { get; set; }
        public Nullable<int> BankId { get; set; }
        public bool IsFinalized { get; set; }
        public string BranchName { get; set; }
        public string ChequeNo { get; set; }
        public string MoneyReceiptName { get; set; }
        public string BankChargeHeadGLName { get; set; }
        public string PaymentToHeadGLName { get; set; }

        public long? CGId { get; set; }
        public string GroupName { get; set; }
        public string MoneyReceiptNo { get; set; }
        public string ReceiveLocation { get; set; }       
        public string PaymentNo { get; set; }
        public Nullable<int> PaymentFromHeadGLId { get; set; }
        public Nullable<int> PaymentToHeadGLId { get; set; }
        public Nullable<int> BankChargeHeadGLId { get; set; }
        public DateTime? MoneyReceiptDate { get; set; }
        public decimal BankCharge { get; set; }
        public decimal TotalAmount { get; set; }
        public int VendorTypeId { get; set; }
        public string VendorTypeName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string StrFromDate { get; set; }
        public string StrToDate { get; set; }
        public IEnumerable<VMPaymentMaster> DataList { get; set; }
    }
}
