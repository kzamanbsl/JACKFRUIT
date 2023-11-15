using KGERP.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace KGERP.Service.ServiceModel
{
    public class EmployeeModel
    {
        public string ButtonName
        {
            get
            {
                return Id > 0 ? "Update" : "Save";
            }

        }
        public long Id { get; set; }

        [DisplayName("Employment Id")]
        public string EmployeeId { get; set; }

        [DisplayName("Manager")]
        public Nullable<long> ManagerId { get; set; }

        [DisplayName("Company")]
        public Nullable<int> CompanyId { get; set; }

        [DisplayName("Card No")]
        public string CardId { get; set; }

        [Required]
        public string Name { get; set; }

        [DisplayName("Nick Name")]
        public string ShortName { get; set; }

        [DisplayName("Present Address")]
        public string PresentAddress { get; set; }

        [DisplayName("Father's Name")]
        public string FatherName { get; set; }

        [DisplayName("Mother's Name")]
        public string MotherName { get; set; }

        [DisplayName("Spouse")]
        public string SpouseName { get; set; }

        [DisplayName("Alt Mobile No")]
        public string Telephone { get; set; }

        [DisplayName("PABX")]
        public string PABX { get; set; }

        [DisplayName("Mobile No")]
        public string MobileNo { get; set; }

        [DisplayName("Fax No")]
        public string FaxNo { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [DisplayName("Personal Email")]
        public string Email { get; set; }

        [DisplayName("Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string OfficeEmail { get; set; }

        [DisplayName("Permanent Address")]
        public string PermanentAddress { get; set; }

        [DisplayName("Department")]
        public Nullable<int> DepartmentId { get; set; }
        public string DepartmentName { get; set; }

        [DisplayName("Designation")]
        public Nullable<int> DesignationId { get; set; }
        public string DesignationName { get; set; }

        [DisplayName("Job Status")]
        public Nullable<int> JobStatusId { get; set; }

        [DisplayName("Joining Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<System.DateTime> JoiningDate { get; set; }


        [DisplayName("Probation End Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<System.DateTime> ProbationEndDate { get; set; }

        [DisplayName("Permanent Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<System.DateTime> PermanentDate { get; set; }

        public bool Active { get; set; }

        [DisplayName("Shift")]
        public Nullable<int> ShiftId { get; set; }
        public string ShiftName { get; internal set; }

        [DisplayName("Date of Birth")]

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? DateOfBirth { get; set; }
        [DisplayName("Date of Marriage")]

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<System.DateTime> DateOfMarriage { get; set; }
        [DisplayName("Grade")]
        public Nullable<int> GradeId { get; set; }
        [DisplayName("Country")]
        public Nullable<int> CountryId { get; set; }
        [DisplayName("Gender")]
        public Nullable<int> GenderId { get; set; }

        [DisplayName("Marital Type")]
        public Nullable<int> MaritalTypeId { get; set; }

        [DisplayName("Division")]
        public Nullable<int> DivisionId { get; set; }
        public string Division { get; set; }

        [DisplayName("District")]
        public Nullable<int> DistrictId { get; set; }

        public string DistrictName { get; set; }
        public string Upzilla { get; set; }
        [DisplayName("Month")]
        public string TotalMonth { get; set; }

        [DisplayName("Upzilla")]
        public Nullable<int> UpzillaId { get; set; }

        [DisplayName("Bank")]
        public Nullable<int> BankId { get; set; }
        public string BankName { get; set; }

        [DisplayName("Bank Branch")]
        public Nullable<int> BankBranchId { get; set; }
        public string BankBranchName { get; set; }

        [DisplayName("Bank Account")]
        public string BankAccount { get; set; }
        [DisplayName("D. License No")]
        public string DrivingLicenseNo { get; set; }
        [DisplayName("Passport No")]
        public string PassportNo { get; set; }
        [DisplayName("Social ID")]
        public string SocialId { get; set; }
        [DisplayName("NID")]
        public string NationalId { get; set; }
        [DisplayName("TIN No")]
        public string TinNo { get; set; }

        

        [DisplayName("Religion")]
        public Nullable<int> ReligionId { get; set; }
        public string Religion { get; set; }

        [DisplayName("Blood Group")]
        public Nullable<int> BloodGroupId { get; set; }

        [DisplayName("Employee Category")]
        public Nullable<int> EmployeeCategoryId { get; set; }
        public string EmployeeCategoryName { get; set; }

        [DisplayName("Service Type")]
        public Nullable<int> ServiceTypeId { get; set; }
        public string ServiceTypeName { get; set; }

        [DisplayName("Disburse Method")]
        public Nullable<int> DisverseMethodId { get; set; }
        public Nullable<int> DesignationFlag { get; set; }

        public string ImageFileName { get; set; }
        [DisplayName("Signature File Name")]
        public string SignatureFileName { get; set; }
      
        [DisplayName("Office Type")]
        public Nullable<int> OfficeTypeId { get; set; }
        public string OfficeTypeName { get; set; }

        [DisplayName("End Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<System.DateTime> EndDate { get; set; }
        [DisplayName("Reason")]
        public string EndReason { get; set; }

        [DisplayName("Employee Order")]
        public int EmployeeOrder { get; set; }

        [DisplayName("Salary Tag")]
        public int SalaryTag { get; set; }
        [DisplayName("Created By")]
        public string CreatedBy { get; set; }
        [DisplayName("Created Date")]
        public Nullable<System.DateTime> CreatedDate { get; set; }
        [DisplayName("Modified By")]
        public string ModifedBy { get; set; }
        [DisplayName("Modified Date")]
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string Remarks { get; set; }
        [DisplayName("Event Date")]
        public string EventDate { get; set; }
        public string EDepartment { get; set; }
        public string EDesignation { get; set; }
        public string Anniversary { get; set; }
        public string ECompany { get; set; }

        [DisplayName("Store Info")]
        public Nullable<int> StockInfoId { get; set; }

        [DataType(DataType.Password)]
        [MinLength(4, ErrorMessage = "Minimum 4 characters required")]
        public string Password { get; set; }

        //-------------Extended Properties----------------
        public string SearchText { get; set; }
        public string StrJoiningDate { get; set; }
      
        public string BloodGroupName { get; set; }
        public string EmployeeIdOfManager { get; set; }
        [Required]
        public string ManagerName { get; set; }
        public long HrAdminId { get; set; }
        public decimal? SalaryAmount { get; set; }
        public string ImagePath { get; set; }
        public string SignaturePath { get; set; }

        [DisplayName("Zone")]
        public int[] ZoneIds { get; set; }

        [DisplayName("Division")]
        public int[] ZoneDivisionIds { get; set; }

        [DisplayName("Area")] 
        public int[] AreaIds { get; set; }

        [DisplayName("Territory")]
        public int[] SubZoneIds { get; set; }



        public virtual StockInfoModel StockInfo { get; set; }
        public virtual BankModel Bank { get; set; }
        public virtual BankBranchModel BankBranch { get; set; }
        public virtual CompanyModel Company { get; set; }
        public virtual DepartmentModel Department { get; set; }
        public virtual DesignationModel Designation { get; set; }
        public virtual DistrictModel District { get; set; }
        public virtual DropDownItemModel DropDownItem { get; set; }
        public virtual DropDownItemModel DropDownItem1 { get; set; }
        public virtual DropDownItemModel DropDownItem2 { get; set; }
        public virtual DropDownItemModel DropDownItem3 { get; set; }
        public virtual DropDownItemModel DropDownItem4 { get; set; }
        public virtual DropDownItemModel DropDownItem5 { get; set; }
        public virtual DropDownItemModel DropDownItem6 { get; set; }
        public virtual DropDownItemModel DropDownItem7 { get; set; }
        public virtual DropDownItemModel DropDownItem8 { get; set; }
        public virtual DropDownItemModel DropDownItem9 { get; set; }
        public virtual EmployeeModel Employee2 { get; set; }

        public virtual EmployeeModel Employee3 { get; set; }
        public virtual ShiftModel Shift { get; set; }

        public virtual ICollection<FileAttachment> FileAttachments { get; set; }
       
    }

    public class EmployeeVm
    {
        public long Id { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public DateTime? JoiningDate { get; set; }
        public string DepartmentName { get; set; }
        public string DesignationName { get; set; }
        public string MobileNo { get; set; }
        public string ServiceArea { get; set; }
        public string Email { get; set; }
        public decimal Samount { get; set; }
        public IEnumerable<EmployeeVm> DataList { get; set; }
    }

    public class EmployeeVmSalary
    {
        public long Id { get; set; }
        public string EmployeeId { get; set; }
        public long EmpId { get; set; }
        public string EmployeeName { get; set; }
        public string Month { get; set; }

        public decimal SamountOwed { get; set; }
        public decimal? Samountpaid { get; set; }
        public decimal? Pay { get; set; } = 0;
        public IEnumerable<EmployeeVmSalary> DataList { get; set; }
        public List<EmployeeVmSalary> MappVm { get; set; }
    }

    


    public class EmployeePaySalary
    {
       public List<EmployeeVmSalary> SalaryList { get; set; }

    }


}
