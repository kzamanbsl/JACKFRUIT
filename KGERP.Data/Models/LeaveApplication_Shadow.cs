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
    
    public partial class LeaveApplication_Shadow
    {
        public long LeaveApplicationId { get; set; }
        public Nullable<long> Id { get; set; }
        public string EmployeeId { get; set; }
        public Nullable<long> ManagerId { get; set; }
        public Nullable<long> HrAdminId { get; set; }
        public string Manager { get; set; }
        public string ManagerName { get; set; }
        public string LeaveType { get; set; }
        public Nullable<int> LeaveCategoryId { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public Nullable<int> LeaveDays { get; set; }
        public Nullable<int> LeaveDue { get; set; }
        public string Address { get; set; }
        public string ContactName { get; set; }
        public string Reason { get; set; }
        public string Remarks { get; set; }
        public Nullable<int> AdminApproved { get; set; }
        public Nullable<int> ManagerApproved { get; set; }
        public Nullable<int> Reject { get; set; }
        public Nullable<int> Active { get; set; }
        public string IP { get; set; }
        public string ManagerStatus { get; set; }
        public string ManagerComment { get; set; }
        public string HrAdminStatus { get; set; }
        public string HrAdminComment { get; set; }
        public Nullable<System.DateTime> ApplicationDate { get; set; }
        public Nullable<System.DateTime> ModifyDate { get; set; }
        public string AuditAction { get; set; }
        public Nullable<System.DateTime> ManagerApprovalDate { get; set; }
        public Nullable<System.DateTime> HRApprovalDate { get; set; }
        public Nullable<int> OperationId { get; set; }
    }
}
