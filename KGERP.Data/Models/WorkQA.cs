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
    
    public partial class WorkQA
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public WorkQA()
        {
            this.WorkQAFiles = new HashSet<WorkQAFile>();
        }
    
        public long WorkQAId { get; set; }
        public Nullable<long> FromEmpId { get; set; }
        public Nullable<long> ToEmpId { get; set; }
        public string Conversation { get; set; }
        public Nullable<System.DateTime> ConversationDate { get; set; }
        public Nullable<long> ParentWorkQAId { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<WorkQAFile> WorkQAFiles { get; set; }
    }
}
