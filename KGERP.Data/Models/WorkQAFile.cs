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
    
    public partial class WorkQAFile
    {
        public long WorkQAFileId { get; set; }
        public Nullable<long> WorkQAId { get; set; }
        public Nullable<long> EmpId { get; set; }
        public string FileName { get; set; }
    
        public virtual WorkQA WorkQA { get; set; }
    }
}
