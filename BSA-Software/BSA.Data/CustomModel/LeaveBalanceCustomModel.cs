using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSA.Data.CustomModel
{
    public class LeaveBalanceCustomModel
    {
        public long Employee { get; set; }
        public int LeaveCategoryId { get; set; }
        public string LeaveCategory { get; set; }
        public int MaxDays { get; set; }
        public int LeaveAvailed { get; set; }
        public string LeaveYear { get; set; }
    }
}
