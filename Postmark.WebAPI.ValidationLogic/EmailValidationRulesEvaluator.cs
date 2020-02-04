using AutoMapper;
using Postmark.Shared;
using Postmark.Shared.Interfaces;
using Postmark.WebAPI.Models;
using Postmark.WebAPI.ValidationLogic.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Postmark.WebAPI.ValidationLogic
{
    public class EmailValidationRulesEvaluator : IEmailValidationRulesEvaluator
    {
        private readonly List<IValidationRule> _validationRules = new List<IValidationRule>();

    
        public void Add(IValidationRule rule)
        {
            if (!_validationRules.Contains(rule))
                _validationRules.Add(rule);
        }


        public EmailResult RunAllRules(ref EmailRequest request)
        {
            var ruleEvalResult = GetDefaultEmailResult(request);

            foreach (var rule in _validationRules)
            {
                //run each rules precondition check
                var preconditionResult = rule.RunPreConditionChecks(ref request);

                switch (preconditionResult.ResultCode)
                {
                    case PreconditionResultCode.FAILED:
                        return preconditionResult.EmailResult;

                    case PreconditionResultCode.SKIP_RULE:
                        continue;

                    case PreconditionResultCode.SUCCESS:
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
                        continue;
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

        public IEmailValidationRulesEvaluator WithRule(IValidationRule rule)
        {
            if (!_validationRules.Contains(rule))
                _validationRules.Add(rule);
            return this;
        }

        public IEmailValidationRulesEvaluator WithRules(IValidationRule[] rules)
        {
            _validationRules.AddRange(rules);
            return this;
        }
    }
}
