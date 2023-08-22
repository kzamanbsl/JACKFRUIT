using BSA.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSA.Service.ServiceModel
{
   public class SeedCategoryModel
    {
        public int Id { get; set; } 

        [DisplayName("Name")]
        public string Name { get; set; }

        [DisplayName("Name Bangla")]
        public string NameBN { get; set; }

        [DisplayName("Member")]
        public string MemberName { get; set; }

        [DisplayName("Member")]
        public string MemberId { get; set; }

        [DisplayName("Type")]
        public string Type { get; set; }
        public string Remarks { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }    
        public List<SelectModel> SeedTypes { get; set; } 

    }
}
