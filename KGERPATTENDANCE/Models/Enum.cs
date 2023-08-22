using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceProcessor.Models
{
    public enum EnumSmSStatus
    {
        Draft = 1,
        Pending = 2,
        Failed = 3,
        Success = 9,
        All = 99,
    }
}
