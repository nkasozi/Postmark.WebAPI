using System;
using System.Collections.Generic;
using System.Text;

namespace Postmark.Shared.Interfaces
{
    public interface IPreconditionResult
    {
        PreconditionResultCode ResultCode { get; set; }
        string ResultDesc { get; set; }
    }
}
