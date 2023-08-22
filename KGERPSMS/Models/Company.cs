using System;
using System.Collections.Generic;

#nullable disable

namespace SmsSchedulerCore.Models
{
    public partial class Company
    {
        public Company()
        {
            ErpSms = new HashSet<ErpSm>();
            InverseParent = new HashSet<Company>();
        }

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
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Param { get; set; }
        public int? LayerNo { get; set; }
        public string CompanyLogo { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsCompany { get; set; }
        public bool IsActive { get; set; }

        public virtual Company Parent { get; set; }
        public virtual ICollection<ErpSm> ErpSms { get; set; }
        public virtual ICollection<Company> InverseParent { get; set; }
    }
}
