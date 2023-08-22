using System;
using System.Collections.Generic;

#nullable disable

namespace SmsSchedulerCore.Models
{
    public partial class SmsType
    {
        public SmsType()
        {
            ErpSms = new HashSet<ErpSm>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<ErpSm> ErpSms { get; set; }
    }
}
