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
    
    public partial class AdvanceSalary
    {
        public int adv_id { get; set; }
        public System.DateTime adv_date { get; set; }
        public string employee_id { get; set; }
        public double adv_amt { get; set; }
        public string remarks { get; set; }
        public int post_flag { get; set; }
        public int save_by { get; set; }
        public System.DateTime save_time { get; set; }
    }
}
