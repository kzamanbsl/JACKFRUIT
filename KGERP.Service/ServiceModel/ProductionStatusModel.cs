using KGERP.Service.Implementation.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KGERP.Service.ServiceModel
{
    public class ProductionStatusModel : BaseVM
    {
        public int ProductionStatusId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsCreateProduct { get; set; }
        public bool IsActive { get; set; }
        public int CompanyId { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
    }
}
