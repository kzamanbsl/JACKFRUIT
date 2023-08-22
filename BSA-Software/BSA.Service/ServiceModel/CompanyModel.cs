using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSA.Service.ServiceModel
{
   public class CompanyModel
    {
        [DisplayName("Company")]
        public int CompanyId { get; set; }
        public string Name { get; set; }
        [DisplayName("Short Name")]
        public string ShortName { get; set; }
        [DisplayName("Company Type")]
        public Nullable<int> CompanyType { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public Nullable<int> Created { get; set; }
        public string MushokNo { get; set; }
        [DisplayName("Created Date")]
        public Nullable<System.DateTime> CreatedDate { get; set; }
    }
}
