using System;
using System.Collections.Generic;
using System.Text;

namespace Postmark.WebAPI.Models
{
    public class SingleEmailResult : EmailResult
    {
        public string To { get; set; }
        public string UniqueEmailID { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string MessageID { get; set; }
        public OpResultCode ErrorCode { get; set; }
        public string Message { get; set; }
    }
}
