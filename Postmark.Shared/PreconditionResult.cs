using Postmark.Shared.Interfaces;
using Postmark.WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Postmark.Shared
{
    public enum PreconditionResultCode
    {
        SUCCESS = 0,
        FAILED = 100,
        SKIP_RULE = 200
    }

    public class PreconditionResult : IPreconditionResult
    {
        public PreconditionResultCode ResultCode { get; set; }
        public string ResultDesc { get; set; }

        public EmailResult EmailResult { get; set; }
    }
}
