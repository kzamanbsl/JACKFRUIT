using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace KGERP.Data.CustomModel
{
    public class ReportCustomModel
    {
        public string Title { get; set; }

        [DisplayName("Company")]
        public int CompanyId { get; set; }

        [DisplayName("A/C No")]
        public int Id { get; set; }
        public long CGId { get; set; }
        public long MoneyReceiptId { get; set; }
        public int? Head2Id { get; set; }
        public int? Head3Id { get; set; }
        public int? Head4Id { get; set; }
        public int? Head5Id { get; set; }
        public int? HeadGLId { get; set; }
        public int? LayerNo { get; set; }
        public string AccCode { get; set; }

        [DisplayName("Voucher No")]
        public string VoucherNo { get; set; }
        public string ReceivedBy { get; set; }
        public int? VmVoucherTypeId { get; set; }
        public string AccName { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        [DisplayName("From Date")]
        public Nullable<System.DateTime> FromDate { get; set; }

        [DisplayName("To Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<System.DateTime> ToDate { get; set; }
        public string ReportType { get; set; }
        public string ReportName { get; set; }
        public string NoteReportName { get; set; }
        public string StockTransferDelivery { get; set; }
        public string StockTransferReceive { get; set; }
        public string StockTransferStock { get; set; }

        [DisplayName("Product Type")]
        public string ProductType { get; set; }
        public string Month { get; set; }

        [DisplayName("From")]
        public string FromMonth { get; set; }

        [DisplayName("To")]
        public string ToMonth { get; set; }
        public string Year { get; set; }

        [DisplayName("Cost Center")]
        public Nullable<int> CostCenterId { get; set; }
        public Nullable<int> StockId { get; set; }
        public Nullable<int> ZoneId { get; set; }
        public Nullable<int> AreaId { get; set; }
        public Nullable<int> TerritoryId { get; set; }

        public List<SelectModel> Years { get; set; }
        public List<SelectModel> Employees { get; set; }
        public List<SelectModel> Vendors { get; set; }
        public SelectList VoucherTypesList { get; set; } = new SelectList(new List<object>());

        public List<SelectModel> Months
        {
            get
            {
                return new List<SelectModel> {
                    new SelectModel { Text="January",Value=1},
                    new SelectModel { Text="February",Value=2},
                    new SelectModel { Text="March",Value=3},
                    new SelectModel { Text="April",Value=4},
                    new SelectModel { Text="May",Value=5},
                    new SelectModel { Text="June",Value=6},
                    new SelectModel { Text="July",Value=7},
                    new SelectModel { Text="August",Value=8},
                    new SelectModel { Text="September",Value=9},
                    new SelectModel { Text="October",Value=10},
                    new SelectModel { Text="November",Value=11},
                     new SelectModel { Text="December",Value=12},
                };
            }
            set { }
        }



        public List<SelectModel> Stocks { get; set; }
        public List<SelectModel> Companies { get; set; }
        public int ProjectId { get; set; }
        public int? VoucherTypeId { get; set; }
        public List<SelectModel> VoucherTypes { get; set; }

        public string Customer { get; set; }
        public string Supplier { get; set; }
        public string EmployeeKGId { get; set; }
        public int? VendorId { get; set; }
        public int Accounting_BankOrCashParantId { get; set; }
        public string AsOnDate { get; set; }
        public long EmployeeId { get; set; }
        public string StrFromDate { get; set; }
        public string StrToDate { get; set; }

        [DisplayName("Product Category")]
        public int? ProductCategoryId { get; set; }

        [DisplayName("Product SubCategory")]
        public int? ProductSubCategoryId { get; set; }

        [DisplayName("Product")]
        public int? ProductId { get; set; }
        public List<SelectModel> CostCenters { get; set; }

        public SelectList Head3List { get; set; } = new SelectList(new List<object>());
        public List<SelectModelType> ProjectList { get; set; }
        public SelectList Head4List { get; set; } = new SelectList(new List<object>());
        public SelectList Head5List { get; set; } = new SelectList(new List<object>());
        public SelectList HeadGLList { get; set; } = new SelectList(new List<object>());


        public SelectList BankOrCashParantList { get; set; } = new SelectList(new List<object>());
        public SelectList BankOrCashGLList { get; set; } = new SelectList(new List<object>());
        public List<SelectModel> ProductCategoryList { get; set; } =new List<SelectModel>();
        public SelectList ProductSubCategoryList { get; set; } = new SelectList(new List<object>());
        public SelectList ProductList { get; set; } = new SelectList(new List<object>());
        public SelectList CostCenterList { get; set; } = new SelectList(new List<object>());
        public SelectList VoucherTypeList { get; set; } = new SelectList(new List<object>());

        public SelectList GroupList { get; set; } = new SelectList(new List<object>());
        public SelectList SupplierList { get; set; } = new SelectList(new List<object>());
        public SelectList Stocklist { get; set; } = new SelectList(new List<object>());
        public List<SelectModelType> ZoneList { get; set; } = new List<SelectModelType> { };
        public SelectList SelectZoneList { get; set; } = new SelectList(new List<object>());
        public SelectList SubZoneList { get; set; } = new SelectList(new List<object>());
        public SelectList CustomerList { get; set; } = new SelectList(new List<object>());
        public int? CustomerId { get; set; }
        public int? StockInfoTypeId { get; set; }
        public int SalaryTag { get; set; } = 0;
        public string AttendanceStatusvalue { get; set; } = "";

        public List<SelectModel> AttendanceStatus
        {
            get
            {
                return new List<SelectModel> {
                    new SelectModel { Text="Present",Value="OK"},
                    new SelectModel { Text="Absent",Value="Absent"},
                    new SelectModel { Text="Late In",Value="Late In"},
                    new SelectModel { Text="Early Out",Value="Early Out"},
                    new SelectModel { Text="Late In & Early Out",Value="Late In & Early Out"},
                    new SelectModel { Text="On Leave",Value="On Leave"},
                    new SelectModel { Text="Tour",Value="Tour"},
                    new SelectModel { Text="On Field",Value="On Field"},
                    new SelectModel { Text="Top Management",Value="Top Management"},
                    new SelectModel { Text="Holiday",Value="Holiday"},
                    new SelectModel { Text="Off Day",Value="Off Day"},

                };
            }
            set { }
        }
        public string SupplierName { get; set; }
        public string CustomerName { get; set; }
        public string CompanyName { get; set; }

        public int? SubZoneFk { get; set; }

        public List<SelectModel> Departments { get; set; }
        public List<SelectModel> Designations { get; set; }
        public int? DepartmentId { get; set; } = 0;
        public int? DesignationId { get; set; } = 0;
    }

    public class ReportCustomerModel
    {
        public int CustomerId { get; set; }
        public int CompanyId { get; set; }
        public List<SelectModel> ZoneList { get; set; } = new List<SelectModel> { };
        public int? ZoneFk { get; set; }
        public int? AreaId { get; set; }
        public int? TerritoryId { get; set; }
        public SelectList SubZoneList { get; set; } = new SelectList(new List<object>());
        public int? SubZoneFk { get; set; }
        public string ReportName { get; set; }
        public string ReportType { get; set; }
    }
}
