using System.Collections.Generic;

namespace Postmark.WebAPI.Models
{
    public class BulkEmail : EmailRequest
    {
        public List<SingleEmail> Emails { get; set; }

    }

   
}
