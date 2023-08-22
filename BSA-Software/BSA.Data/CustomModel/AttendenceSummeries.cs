using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSA.Data.CustomModel
{
    public class AttendenceSummeries
    {
        [DisplayName("Id")]
        public string EmployeeId { get; set; }
        public string Name { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public int? Absent { get; set; }
        [DisplayName("Late In")]
        public int? LateIn { get; set; }
        [DisplayName("Early Out")]
        public int? EarlyOut { get; set; }
        [DisplayName("Late In & Early Out")]
        public int? LateInEarlyOut { get; set; }
        [DisplayName("Present")]
        public int? OK { get; set; }
    }
}
