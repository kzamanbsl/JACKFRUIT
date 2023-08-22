using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmsSchedulerCore.Models
{
    public class SingleSMSResp
    {
            public string request_type { get; set; }
            public string campaign_uid { get; set; }
            public string sms_uid { get; set; }
            public List<string> invalid_numbers { get; set; }
            public int api_response_code { get; set; }
            public string api_response_message { get; set; }
    }
    public class MultiSmsSuccessResponse
    {
        public string request_type { get; set; }
        public string campaign_uid { get; set; }
        public object sms_uid { get; set; }
        public List<string> invalid_numbers { get; set; }
        public int api_response_code { get; set; }
        public string api_response_message { get; set; }
    }

    public class Error
    {
        public int error_code { get; set; }
        public string error_message { get; set; }
    }

    public class MultiSmsFailedResponse
    {
        public string request_type { get; set; }
        public int api_response_code { get; set; }
        public string api_response_message { get; set; }
        public Error error { get; set; }
    }
}
