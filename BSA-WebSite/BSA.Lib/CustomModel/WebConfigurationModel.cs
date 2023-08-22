using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bsa.lib.Utility;
using System.Web;
using System.Web.Mvc;

namespace Bsa.lib.CustomModel
{
    //public class WebConfigurationModel
    // {

    // }
    public abstract class BaseVM
    {
        public int ID { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public ActionEnum ActionEum { get { return (Utility.ActionEnum)this.ActionId; } }
        public int ActionId { get; set; } = 1;
        public bool IsActive { get; set; }
        public int? CompanyFK { get; set; }
        public string Remarks { get; set; }
        public string Code { get; set; }
    }

    public class MdSecretaryMessageModel : BaseVM
    {
        public string Message { get; set; }
        public string Title { get; set; }
        public IEnumerable<MdSecretaryMessageModel> DataList { get; set; }
        public SelectList CompanyList { get; set; } = new SelectList(new List<object>());
    }
}
