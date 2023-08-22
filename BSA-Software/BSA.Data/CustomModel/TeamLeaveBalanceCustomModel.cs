using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSA.Data.CustomModel
{
    public class TeamLeaveBalanceCustomModel
    {
        public string EmployeeId { get; set; }
        public string Name { get; set; }
        public int CLAvailed { get; set; }
        public int CLDue { get; set; }
        public int ELAvailed { get; set; }
        public int ELDue { get; set; }
        public int MrgAvailed { get; set; }
        public int MrgDue { get; set; }
        public int MtrAvailed { get; set; }
        public int MtrDue { get; set; }
        public int SelAvailed { get; set; }
        public int SelDue { get; set; }
        public int HjAvailed { get; set; }
        public int HjDue { get; set; }
        public int LwAvailed { get; set; }
        public int LwDue { get; set; }
        public int CLASDGAvailed { get; set; }
        public int CLASDGDue { get; set; }
    }
}
