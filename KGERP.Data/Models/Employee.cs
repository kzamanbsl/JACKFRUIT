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
    
    public partial class Employee
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Employee()
        {
            this.AssetAssigns = new HashSet<AssetAssign>();
            this.Educations = new HashSet<Education>();
            this.Employee1 = new HashSet<Employee>();
            this.Employee11 = new HashSet<Employee>();
            this.EmployeeAttendances = new HashSet<EmployeeAttendance>();
            this.FileAttachments = new HashSet<FileAttachment>();
            this.LeaveApplications = new HashSet<LeaveApplication>();
            this.LeaveApplications1 = new HashSet<LeaveApplication>();
            this.LeaveApplications2 = new HashSet<LeaveApplication>();
            this.OfficerAssigns = new HashSet<OfficerAssign>();
            this.SaleReturns = new HashSet<SaleReturn>();
            this.TeamInfoes = new HashSet<TeamInfo>();
            this.Upazilas = new HashSet<Upazila>();
            this.UpazilaAssigns = new HashSet<UpazilaAssign>();
            this.Works = new HashSet<Work>();
            this.WorkAssigns = new HashSet<WorkAssign>();
            this.ManagerProductMaps = new HashSet<ManagerProductMap>();
            this.ExpenseMasters = new HashSet<ExpenseMaster>();
            this.SubZones = new HashSet<SubZone>();
            this.Zones = new HashSet<Zone>();
            this.Regions = new HashSet<Region>();
            this.Areas = new HashSet<Area>();
        }
    
        public long Id { get; set; }
        public string EmployeeId { get; set; }
        public Nullable<long> ManagerId { get; set; }
        public Nullable<long> HrAdminId { get; set; }
        public Nullable<int> CompanyId { get; set; }
        public string CardId { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public Nullable<System.DateTime> DateOfBirth { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string SpouseName { get; set; }
        public Nullable<System.DateTime> DateOfMarriage { get; set; }
        public string NationalId { get; set; }
        public Nullable<int> GenderId { get; set; }
        public Nullable<int> MaritalTypeId { get; set; }
        public Nullable<int> ReligionId { get; set; }
        public Nullable<int> BloodGroupId { get; set; }
        public string MobileNo { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string OfficeEmail { get; set; }
        public string FaxNo { get; set; }
        public string PABX { get; set; }
        public string DrivingLicenseNo { get; set; }
        public string PassportNo { get; set; }
        public string TinNo { get; set; }
        public string SocialId { get; set; }
        public string PresentAddress { get; set; }
        public string PermanentAddress { get; set; }
        public Nullable<int> DivisionId { get; set; }
        public Nullable<int> DistrictId { get; set; }
        public Nullable<int> UpzillaId { get; set; }
        public Nullable<int> CountryId { get; set; }
        public Nullable<System.DateTime> JoiningDate { get; set; }
        public Nullable<System.DateTime> ProbationEndDate { get; set; }
        public Nullable<System.DateTime> PermanentDate { get; set; }
        public Nullable<int> DepartmentId { get; set; }
        public Nullable<int> DesignationId { get; set; }
        public Nullable<int> EmployeeCategoryId { get; set; }
        public Nullable<int> ServiceTypeId { get; set; }
        public Nullable<int> JobStatusId { get; set; }
        public Nullable<int> OfficeTypeId { get; set; }
        public Nullable<int> BankId { get; set; }
        public Nullable<int> BankBranchId { get; set; }
        public string BankAccount { get; set; }
        public Nullable<int> ShiftId { get; set; }
        public Nullable<int> GradeId { get; set; }
        public Nullable<int> DisverseMethodId { get; set; }
        public Nullable<int> DesignationFlag { get; set; }
        public string ImageFileName { get; set; }
        public string SignatureFileName { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public string EndReason { get; set; }
        public string Remarks { get; set; }
        public int EmployeeOrder { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string ModifedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public bool Active { get; set; }
        public Nullable<int> SalaryTag { get; set; }
        public Nullable<decimal> SalaryAmount { get; set; }
        public Nullable<int> StockInfoId { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AssetAssign> AssetAssigns { get; set; }
        public virtual Bank Bank { get; set; }
        public virtual BankBranch BankBranch { get; set; }
        public virtual Company Company { get; set; }
        public virtual District District { get; set; }
        public virtual Division Division { get; set; }
        public virtual DropDownItem DropDownItem { get; set; }
        public virtual DropDownItem DropDownItem1 { get; set; }
        public virtual DropDownItem DropDownItem2 { get; set; }
        public virtual DropDownItem DropDownItem3 { get; set; }
        public virtual DropDownItem DropDownItem4 { get; set; }
        public virtual DropDownItem DropDownItem5 { get; set; }
        public virtual DropDownItem DropDownItem6 { get; set; }
        public virtual DropDownItem DropDownItem7 { get; set; }
        public virtual DropDownItem DropDownItem8 { get; set; }
        public virtual DropDownItem DropDownItem9 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Education> Educations { get; set; }
        public virtual Grade Grade { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Employee> Employee1 { get; set; }
        public virtual Employee Employee2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Employee> Employee11 { get; set; }
        public virtual Employee Employee3 { get; set; }
        public virtual Shift Shift { get; set; }
        public virtual Upazila Upazila { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EmployeeAttendance> EmployeeAttendances { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FileAttachment> FileAttachments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LeaveApplication> LeaveApplications { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LeaveApplication> LeaveApplications1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LeaveApplication> LeaveApplications2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OfficerAssign> OfficerAssigns { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SaleReturn> SaleReturns { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TeamInfo> TeamInfoes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Upazila> Upazilas { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UpazilaAssign> UpazilaAssigns { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Work> Works { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<WorkAssign> WorkAssigns { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ManagerProductMap> ManagerProductMaps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ExpenseMaster> ExpenseMasters { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SubZone> SubZones { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Zone> Zones { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Region> Regions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Area> Areas { get; set; }
        public virtual Department Department { get; set; }
        public virtual Designation Designation { get; set; }
        public virtual StockInfo StockInfo { get; set; }
    }
}
