//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BSA.Data.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Education
    {
        public int EducationId { get; set; }
        public Nullable<long> Id { get; set; }
        public string EmployeeId { get; set; }
        public string Examination { get; set; }
        public Nullable<int> ExaminationId { get; set; }
        public string Subject { get; set; }
        public string Institute { get; set; }
        public string PassingYear { get; set; }
        public string RollNo { get; set; }
        public string RegNo { get; set; }
        public string Result { get; set; }
        public string Remarks { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
    
        public virtual DropDownItem DropDownItem { get; set; }
    }
}
