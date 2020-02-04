using System;
using System.Collections.Generic;
using System.Text;

namespace Postmark.WebAPI.Models
{
    public class BulkEmail : EmailRequest
    {
        public List<SingleEmail> Emails { get; set; }

    }

   
}
