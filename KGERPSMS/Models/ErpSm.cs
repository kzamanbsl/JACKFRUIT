using System;
using System.Collections.Generic;

#nullable disable

namespace SmsSchedulerCore.Models
{
    public partial class ErpSm
    {
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public string Subject { get; set; }
        public string PhoneNo { get; set; }
        public string Message { get; set; }
        public int CompanyId { get; set; }
        public int? Status { get; set; }
        public int TryCount { get; set; }
        public DateTime RowTime { get; set; }
        public string Remarks { get; set; }
        public int SmsType { get; set; }

        public virtual Company Company { get; set; }
        public virtual SmsType SmsTypeNavigation { get; set; }
    }
}
