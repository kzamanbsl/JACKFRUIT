using KGERP.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace KGERP.Service.ServiceModel
{
    public class ManagerProductMapModel
    {
        public long ManagerProductMapId { get; set; }
        public long? EmployeeId { get; set; }

        [NotMapped]
        public string EmployeeName { get; set; }
        public int? ProductId { get; set; }
        [NotMapped]
        public string ProductName { get; set; }
        [NotMapped]
        public string ProductCategoryName { get; set; }
        [NotMapped]
        public string ProductSubCategoryName { get; set; }
        public int CompanyId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        [DisplayName("Active")]
        public bool IsActive { get; set; }

        public IEnumerable<ManagerProductMapModel> DataList { get; set; } = new List<ManagerProductMapModel>();
        public List<SelectModel> EmployeeList { get; set; } = new List<SelectModel>();
        public List<SelectDDLModel> ProductList { get; set; } = new List<SelectDDLModel>();

    }
}
