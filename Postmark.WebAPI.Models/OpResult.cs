using System;
using System.Collections.Generic;
using System.Text;

namespace Postmark.WebAPI.Models
{
    public enum OpResultCode
    {
        SUCCCESS = 0,
        FAILURE = 100
    }
    public class OpResult
    {
        public OpResultCode ResultCode { get; set; }
        public string StatusDesc;

    }

   
}
