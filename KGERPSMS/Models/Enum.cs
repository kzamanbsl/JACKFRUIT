using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsSchedulerCore.Models
{
    public enum EnumSmSStatus
    {
        Draft = 1,
        Pending = 2,
        Failed = 3,
        Cancel=4,
        Success = 9,
        All = 99,
    }
}
