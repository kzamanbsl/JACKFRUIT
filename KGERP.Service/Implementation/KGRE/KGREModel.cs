﻿using KGERP.Data.Models;
using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace KGERP.Service.Implementation.KGRE
{
    public abstract class BaseVM
    {
        public int ID { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public ActionEnum ActionEum { get { return (ActionEnum)this.ActionId; } }
        public int ActionId { get; set; } = 1;
        public JournalEnum JournalEnum { get { return (JournalEnum)this.JournalType; } }
        public int JournalType { get; set; } = (int)JournalEnum.JournalVoucher;
        public bool IsActive { get; set; }
        public int? CompanyFK { get; set; }
        public string Remarks { get; set; }
        public string Code { get; set; }
    }
    public class CompanyVM : BaseVM
    {
        public int CompanyId { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int OrderNo { get; set; }
        public int? CompanyType { get; set; }
        public string MushokNo { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public int? LayerNo { get; set; }
        public string CompanyLogo { get; set; }
        public bool IsCompany { get; set; }
        public IEnumerable<CompanyVM> DataList { get; set; }
        public HttpPostedFileBase CompanyLogoUpload { get; set; }

        public string Controller { get; set; }
        public string Action { get; set; }
        public string Param { get; set; }

    }
    public class KGREPaymentVM : BaseVM
    {
        public IEnumerable<KGREPaymentVM> DataList { get; set; }
        public int TransactionId { get; set; }
        public string FileNo { get; set; }
        public Nullable<int> ProjectId { get; set; }
        public Nullable<int> PlotId { get; set; }
        public Nullable<int> InstallmentId { get; set; }
        public Nullable<System.DateTime> TransactionDate { get; set; }
        public Nullable<int> PaymentFor { get; set; }
        public Nullable<double> Amount { get; set; }
        public Nullable<int> BankId { get; set; }
        public string BankName { get; set; }
        public string BranchName { get; set; }
        public string ChkNo { get; set; }
        public string ChkName { get; set; }
        public Nullable<System.DateTime> ChkDate { get; set; }
        public string Notes { get; set; }
    }


    public class KGRECRMVM : BaseVM
    {
        public IEnumerable<KGRECRMVM> DataList { get; set; }
        public string ButtonName
        {
            get
            {
                return ClientId > 0 ? "Modify" : "Save";
            }
        }
        public bool SaveStatus { get; set; }
        public int ClientId { get; set; }
        public int? CompanyId { get; set; }
        [DisplayName("Job Title")]
        public string Designation { get; set; }
        [Required]
        [DisplayName("Name (English)")]
        public string FullName { get; set; }

        [DisplayName("Name (Bangla)")]
        public string BanglaName { get; set; }


        [DisplayName("Reg's Name")]
        public string RegistrationName { get; set; }

        [DisplayName("Father Name")]
        public string FatherName { get; set; }
        [DisplayName("Mother Name")]
        public string MotherName { get; set; }
        public string Spouse { get; set; }

        [DisplayName("Dealing Officer")]
        public string DealingOfficer { get; set; }
        [DisplayName("EmployeeId")]
        public string EmployeeId { get; set; }
        [DisplayName("Organization")]
        public string DepartmentOrInstitution { get; set; }
        [DisplayName("Present Address")]
        public string PresentAddress { get; set; }
        [DisplayName("Permanent Address")]
        public string PermanentAddress { get; set; }

        [DisplayName("Date of Birth")]
        //[DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<System.DateTime> DateofBirth { get; set; }

        [DisplayName("First Contact Date")]
        //[DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<System.DateTime> DateOfContact { get; set; }

        [DisplayName("Next Followup Date")]
        //[DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<System.DateTime> NextFollowupdate { get; set; }

        [DisplayName("Last Contact Date")]

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<System.DateTime> LastContactDate { get; set; }
        public string Nationality { get; set; }
        [DisplayName("Campaign Name")]
        public string CampaignName { get; set; }

        [DisplayName("Mobile No")]
        [Required]

        [RegularExpression("^[0-9]*$", ErrorMessage = "Numeric Only")]
        public string MobileNo { get; set; }


        [RegularExpression("^[0-9]*$", ErrorMessage = "Numeric Only")]
        [DisplayName("Mobile No 2")]
        public string MobileNo2 { get; set; }

        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",
            ErrorMessage = "Please enter a valid e-mail adress")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",
            ErrorMessage = "Please enter a valid e-mail adress")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email 2")]
        public string Email1 { get; set; }

        [DisplayName("NID No")]
        public string NID { get; set; }
        [DisplayName("Birth ID")]
        public string BID { get; set; }

        [Required]
        [DisplayName("File No")]
        public string FileNo { get; set; }
        public string Gender { get; set; }
        public string Religion { get; set; }
        [DisplayName("Religion")]
        public Nullable<int> ReligionId { get; set; }
        public string Profession { get; set; }
        public string Fax { get; set; }
        [DisplayName("TIN No")]
        public string TIN { get; set; }

        [DisplayName("Telephone(Res)")]
        public string TelephoneRes { get; set; }
        [DisplayName("Telephone(Off)")]
        public string TelephoneOffice { get; set; }

        [DisplayName("Office Address")]
        public string OfficeAddress { get; set; }

        [DisplayName("Passport No")]
        public string PassportNo { get; set; }
        [DisplayName("Source of Media")]
        public string SourceOfMedia { get; set; }
        [DisplayName("Promotional Offer")]
        public string PromotionalOffer { get; set; }

        [DisplayName("Service Status")]
        public string StatusLevel { get; set; }
        [DisplayName("Type of Interest")]
        public string TypeOfInterest { get; set; }
        [DisplayName("Project")]
        public string ProjectName { get; set; }

        public Nullable<int> ProjectId { get; set; }
        public string Project { get; set; }
        [DisplayName("Choice of Area")]
        public string ChoieceOfArea { get; set; }
        [DisplayName("Dealing Officer")]
        [Required]
        public string ResponsibleOfficer { get; set; }
        [DisplayName("Referred By")]
        public string ReferredBy { get; set; }
        [DisplayName("Service Detail")]
        public string ServicesDescription { get; set; }


        [DisplayName("Next Project Visit Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]

        public Nullable<System.DateTime> NextScheduleDate { get; set; }
        [DisplayName("Last Meeting Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]

        public Nullable<System.DateTime> LastMeetingDate { get; set; }


        [DisplayName("From Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public Nullable<System.DateTime> FromDate { get; set; }

        [DisplayName("To Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public Nullable<System.DateTime> ToDate { get; set; }

        //Duplicate Data Binding


        #region // Booking

        [DisplayName("Bank Draft")]
        public string BankDraftPayOrdChq { get; set; }
        [DisplayName("Name")]
        public string NomineeFullName { get; set; }
        [DisplayName("Father's Name")]
        public string NomineeFathersName { get; set; }
        [DisplayName("Mother's Name")]
        public string NomineeMothersName { get; set; }
        [DisplayName("Address")]
        public string NomineePerAdderss { get; set; }
        [DisplayName("Relation")]
        public string ReletionwithApplicant { get; set; }
        [DisplayName("Nationlaty")]
        public string NomineeNationlaty { get; set; }
        [DisplayName("NID")]
        public string NomineeNID { get; set; }
        [DisplayName("Tel/Mobile No")]
        public string NomineeTELMobile { get; set; }


        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",
    ErrorMessage = "Please enter a valid e-mail adress")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string NomineeEmail { get; set; }

        [DisplayName("Client Photo")]
        public string ClientImageLink { get; set; }
        [DisplayName("Nominee Photo")]
        public string NomineeImageLink { get; set; }

        [DisplayName("Per Katha")]
        public double LandPricePerKatha { get; set; }
        [DisplayName("Land Value")]
        public string LandValue { get; set; }
        public string Discount { get; set; }
        [DisplayName("Additional 25%")]
        public string Additional25Percent { get; set; }
        [DisplayName("Additional 15%")]
        public double Additional15Percent { get; set; }
        [DisplayName("Additional 10%")]
        public double Additional10Percent { get; set; }
        [DisplayName("After Discount")]
        public double LandValueAfterDiscount { get; set; }
        [DisplayName("Additional Cost")]
        public double AdditionalCost { get; set; }
        [DisplayName("OtherCost")]
        public double OtherCostName { get; set; }
        [DisplayName("Utility Cost")]
        public double UtilityCost { get; set; }

        [DisplayName("Grand Total")]
        public double GrandTotal { get; set; }
        [DisplayName("Booking Money")]
        public double BookingMoney { get; set; }
        [DisplayName("Rest Of Amount")]
        public double RestOfAmount { get; set; }
        [DisplayName("Installment Amount")]
        public int InstallmentAmount { get; set; }

        [DisplayName("No of Installment")]
        public int NoOfInstallment { get; set; }
        public string Remarks { get; set; }
        [DisplayName("Nominee Notes")]
        public string NomineeNote { get; set; }
        [DisplayName("Booking Notes")]
        public string BookingRemarks { get; set; }
        [Required]
        [DisplayName("Booking Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<System.DateTime> BookingDate { get; set; }
        [DisplayName("Pay Type")]
        public string PayType { get; set; }
        [DisplayName("Bank Name")]
        public string BankName { get; set; }
        [DisplayName("Chaque No")]
        public string ChaqueNo { get; set; }

        [DisplayName("Sales Type")]
        public int SalesTypeId { get; set; }

        #endregion


        [DisplayName("Upload File")]
        public string AttachFilePath { get; set; }
        public HttpPostedFileBase AttachFile { get; set; }
        public object ErrorMsg { get; set; }
        public List<SelectModel> SourceOfMedias { get; set; }
        public List<SelectModel> PromotionalOffers { get; set; }
        public List<SelectModel> ResponsiblePersons { get; set; }
        public List<SelectModel> PlotFlats { get; set; }
        public List<SelectModel> Impressions { get; set; }
        public List<SelectModel> StatusLevels { get; set; }
        public List<SelectModel> Genders { get; set; }
        public List<SelectModel> KGREProjects { get; set; }
        public List<SelectModel> KGREChoiceAreas { get; set; }
        public virtual ICollection<FileAttachment> FileAttachments { get; set; }
    }

}
