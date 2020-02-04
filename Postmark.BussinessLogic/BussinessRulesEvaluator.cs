using Postmark.BussinessLogic.Interfaces;
using Postmark.WebAPI.Models;
using System.Collections.Generic;
using System.Linq;

namespace Postmark.BussinessLogic
{
    public class BussinessRulesEvaluator : IBussinessRulesEvaluator
    {
        private List<IEmailBussinessRule> _rules = new List<IEmailBussinessRule>();

        public void Add(IEmailBussinessRule rule)
        {
            if (!_rules.Contains(rule))
                _rules.Add(rule);
        }

        public IBussinessRulesEvaluator WithRule(IEmailBussinessRule rule)
        {
            if (!_rules.Contains(rule))
                _rules.Add(rule);

            return this;
        }

        public IBussinessRulesEvaluator WithRules(IEmailBussinessRule[] rule)
        {
            _rules.AddRange(rule);
            return this;
        }

        public EmailResult RunAllRules(ref EmailRequest request)
        {
            var ruleEvalResult = GetDefaultEmailResult(request);

            foreach (var rule in _rules)
            {
                //run each rules precondition check
                ruleEvalResult = rule.RunRuleChecks(ref request);

                switch (ruleEvalResult)
                {
                    case SingleEmailResult singleEmailResult:
                        if (singleEmailResult.ErrorCode != OpResultCode.SUCCCESS)
                            return singleEmailResult;
                        break;

                    case BulkEmailResult bulkEmailResult:

                        //so for bulk email result we have to do some juggling
                        //if an email in the bulk emails fails a rule check
                        //we remove it from the remaining bulk emails on
                        //the next rule run
                        //basically only emails that pass a rule check can be checked by 
                        //subsequent rule checks

                        var failedResults = bulkEmailResult.Results.FindAll(i => i.ErrorCode != OpResultCode.SUCCCESS);
                        var bulkEmailRequest = request as BulkEmail;

                        request = new BulkEmail
                        {
                            Emails = GetRemainingEmailsThatPassedRuleCheck(failedResults, bulkEmailRequest)
                        };

                        BulkEmails.AddRange(failedResults.ToList());
                        break;


                }
            }

            switch (ruleEvalResult)
            {
                case BulkEmailResult bulkEmailResult:
                    BulkEmails.AddRange(bulkEmailResult.Results);
                    return BulkEmails;
            }

            return ruleEvalResult;
        }




        private List<SingleEmail> GetRemainingEmailsThatPassedRuleCheck(List<SingleEmailResult> failedResults, BulkEmail bulkEmailRequest)
        {

            foreach (var email in failedResults)
            {
                var emailResult = bulkEmailRequest.Emails.FirstOrDefault(i => i.UniqueEmailID == email.UniqueEmailID);
                if (emailResult != null)
                    bulkEmailRequest.Emails.Remove(emailResult);
            }

            return bulkEmailRequest.Emails;
        }

        private readonly BulkEmailResult BulkEmails = new BulkEmailResult();

        private EmailResult GetDefaultEmailResult(EmailRequest request)
        {
            switch (request)
            {
                case SingleEmail singleEmail:
                    return GetSingleEmailResult(singleEmail);

                default:
                    return GetBulkEmailResult(request);
            }
        }

        private EmailResult GetBulkEmailResult(EmailRequest request)
        {
            BulkEmail bulkEmail = request as BulkEmail;
            BulkEmailResult bulkEmailResult = new BulkEmailResult();
            foreach (var email in bulkEmail.Emails)
            {
                var singleEmailResult = GetSingleEmailResult(email);
                bulkEmailResult.Results.Add(singleEmailResult);
            }
            return bulkEmailResult;
        }

        private SingleEmailResult GetSingleEmailResult(SingleEmail singleEmail)
        {
            return new SingleEmailResult
            {
                ErrorCode = OpResultCode.SUCCCESS,
                UniqueEmailID = singleEmail.UniqueEmailID,
                Message = OpResultCode.SUCCCESS.ToString(),
                To = singleEmail.To
            };
        }
    }
}
