using BSA.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace BSA.Service.ServiceModel
{
    public class CustomerModel
    {
        public string ButtonName
        {
            get
            {
                return OID > 0 ? "Update" : "Save";
            }
        }
         
        public int OID { get; set; }
        [Required]

        [DisplayName("Customer Id")]
        public string CustomerId { get; set; }

        [DisplayName("Member Name")]
        public string MemberName { get; set; }


        [DisplayName("Seed Reg. No")]
        public string RegistrationNo { get; set; }

        [DisplayName("Member No")]
        public string MemberNo { get; set; }
        [Required]
        [DisplayName("Customer Type")]
        public string CustomerType { get; set; }
        [DisplayName("Customer Name")]
        [Required]
        public string CustomerName { get; set; }

        [DisplayName("Contact Person")]
        public string ContactPersonName { get; set; }
        [DisplayName("Date of Birth")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd-MMM-yyyy}")]
        public Nullable<System.DateTime> DateOfBirth { get; set; }
        public string Nationality { get; set; }
        public string Telephone { get; set; }
        [Required]
        [DisplayName("Mobile No")]
        public string MobileNo { get; set; }
        public string Email { get; set; }
        [DisplayName("Email")]
        public string ConcatPersonEmail { get; set; }

        [DisplayName("Contact Person Photo")]
        public string ImageUrl { get; set; }

        [DisplayName("Company Logo")]
        public string LogoUrl { get; set; }
        [Required]
        [DisplayName("NID/BIN/TIN")]
        public string NIDorBIN { get; set; }
        public string TIN { get; set; }

        [DisplayName("Company Address")]
        public string AddressOne { get; set; }
        [DisplayName("Address")]
        public string AddressTwo { get; set; }
        public string Division { get; set; }
        public string District { get; set; }
        public string Upzilla { get; set; }

        [DisplayName("Division")]
        public Nullable<int> DivisionId { get; set; }
        [DisplayName("District")]
        public Nullable<int> DistrictId { get; set; }
        [DisplayName("Upzilla")]
        public Nullable<int> UpzillaId { get; set; }

        public string Designation { get; set; }
        [DisplayName("Date Of Registration")]
        public Nullable<System.DateTime> DateOfRegistration { get; set; }
        [DisplayName("Corporate Office")]
        public string CorporateOffice { get; set; }
        [DisplayName("Fiscal Year")]
        public string FiscalYear { get; set; }
        [DisplayName("Trade License")]
        public string TradeLicense { get; set; }
        [DisplayName("Website URL")]
        public string Website { get; set; }
        [DisplayName("Payment Status")]
        public string PaymentStatus { get; set; }
        [DisplayName("Over Due Ammount")]
        public Nullable<decimal> OverDuePayment { get; set; }
        [DisplayName("Due Ammount")]
        public Nullable<decimal> DuePayment { get; set; }
        [DisplayName("Last Payment Date")]
        public Nullable<System.DateTime> LastPaymentDate { get; set; }
        [DisplayName("Customer Status")]
        public string CustomerStatus { get; set; }
        public string Remarks { get; set; }
        public Nullable<bool> Active { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
      
        public List<SelectModel> CustStatus { get; set; }
        public List<SelectModel> PaymentsStatus { get; set; }
        public List<SelectModel> CustomerTypes { get; set; }
        public List<SelectModel> Districts { get; set; }

        [DisplayName("Upload File")]
        public string FilePath { get; set; }
        public HttpPostedFileBase ExcelFile { get; set; }

    }
}
