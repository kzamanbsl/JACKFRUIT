using BSA.Data.CustomModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSA.Data.CustomViewModel
{
  public  class EmployeeLeaveBalanceCustomModel
    {
        public IEnumerable<LeaveBalanceCustomModel> LeaveBalanceCustomModels { get; set; }
        public EmployeeCustomModel EmployeeCustomModel { get; set; }
    }
}
