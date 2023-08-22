using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSA.Service.ServiceModel
{
    public class AdminSetUpModel
    {
        public long AdminId { get; set; }
        [DisplayName("Employee")]
        public long Id { get; set; }
        [DisplayName("Created By")]
        public string CreatededBy { get; set; }
        [DisplayName("Created Date")]
        public System.DateTime CreatedDate { get; set; }
        [DisplayName("Modified By")]
        public string ModifiedBy { get; set; }
        [DisplayName("Modified Date")]
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        [DisplayName("Status")]
        public bool IsActive { get; set; }

        public virtual MemberModel Employee { get; set; }
    }
}
