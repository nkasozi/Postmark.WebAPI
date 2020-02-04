using System;
using System.Collections.Generic;
using System.Text;

namespace Postmark.WebAPI.Models
{
    public class BulkEmailResult: EmailResult
    {
        public List<SingleEmailResult> Results { get; }

        public BulkEmailResult()
        {
            Results = new List<SingleEmailResult>();
        }

        public void AddRange(IEnumerable<SingleEmailResult> singleEmailResults)
        {
            foreach(var email in singleEmailResults)
            {
                if (!Results.Exists(i=>i.UniqueEmailID==email.UniqueEmailID)) 
                    Results.Add(email);
            };
        }
    }
}
