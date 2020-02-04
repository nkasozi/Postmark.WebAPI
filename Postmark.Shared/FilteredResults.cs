using Postmark.WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Postmark.Shared
{
    public class FilteredResults
    {
        public List<ProcessedEmail> ProcessedEmails { get; set; }

        public RulesResultCode ResultCode { get; set; }

        public string ResultDesc { get; set; }

        public FilteredResults()
        {

        }

        public FilteredResults(EmailRequest request)
        {
            ProcessedEmails = new List<ProcessedEmail>();

            if (request == null) return;

            switch (request)
            {

                case SingleEmail singleEmail:
                    var processedEmail = GetProcessedEmail(singleEmail);
                    ProcessedEmails.Add(processedEmail);
                    break;

                case BulkEmail bulkEmail:
                    foreach(var email in bulkEmail.Emails)
                    {
                        var processedEmail2 = GetProcessedEmail(email);
                        ProcessedEmails.Add(processedEmail2);
                    }
                    break;

            }
        }



        private static ProcessedEmail GetProcessedEmail(SingleEmail singleEmail)
        {
            return new ProcessedEmail
            {
                SingleEmail = singleEmail,
                ResultCode = RulesResultCode.SUCCESS,
                ResultDesc = RulesResultDesc.NO_RULES_RUN
            };
        }
    }

    public class ProcessedEmail
    {
        public SingleEmail SingleEmail { get; set; }
        public RulesResultCode ResultCode { get; set; }
        public string ResultDesc { get; set; }
    }
}
