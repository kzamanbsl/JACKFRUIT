using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsSchedulerCore.Models
{
    public class AppSetting {
        public APIInfo SmsAPi { get; set; }
        public ConnectionStrings ConnectionStrings { get; set; }
    }

    public class APIInfo
    {
        public string baseUrl { get; set; }
        public string api_key { get; set; }
        public string api_secret { get; set; }
        public string postUrl { get; set; }
        public int retry { get; set; } = 3;

    }
    public class ConnectionStrings
    {
        public string KGERPDB { get; set; }
    }
}
