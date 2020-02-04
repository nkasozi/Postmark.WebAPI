using Postmark.BussinessLogic.Interfaces;
using Postmark.WebAPI.Models;
using System;
using System.Collections.Generic;

namespace Postmark.BussinessLogic
{
    public class UnsubscribeRule : IEmailBussinessRule
    {
        public List<string> UnbsubscribedEmails = new List<string>();

        public const string ERROR_MESSAGE = "User is Unsubscribed";

        public EmailResult RunRuleChecks(ref EmailRequest obj)
        {
            EmailResult ruleEvaluationResult = null;

            switch (obj)
            {
                case SingleEmail singleEmail:
                    ruleEvaluationResult = RunRuleOnSingleEmail(singleEmail);
                    break;

                case BulkEmail bulkEmail:
                    ruleEvaluationResult = RunRuleOnBulkEmail(bulkEmail);
                    break;

            }

            return ruleEvaluationResult;
        }

        private SingleEmailResult RunRuleOnSingleEmail(SingleEmail singleEmail)
        {
            //IRuleEvaluationResult<EmailResult> ruleEvaluationResult = new EmailRuleEvaluationResult();

            var singleEmailResult = checkIfAlreadyUnsubcribed(singleEmail);

            return singleEmailResult;

        }

        private BulkEmailResult RunRuleOnBulkEmail(BulkEmail bulkEmail)
        {
            BulkEmailResult bulkEmailResult = new BulkEmailResult();

            foreach (var singleEmail in bulkEmail.Emails)
            {
                var singleEmailResult = checkIfAlreadyUnsubcribed(singleEmail);

                bulkEmailResult.Results.Add(singleEmailResult);
                
            }

            return bulkEmailResult;

        }

        private SingleEmailResult checkIfAlreadyUnsubcribed(SingleEmail singleEmail)
        {
            var emailResult = new SingleEmailResult();
            //user unsubscribed
            if (UnbsubscribedEmails.Contains(singleEmail.To))
            {
                emailResult.To = singleEmail.To;
                emailResult.Message = ERROR_MESSAGE;
                emailResult.SubmittedAt = DateTime.Now;
                emailResult.UniqueEmailID = singleEmail.UniqueEmailID;
                emailResult.ErrorCode = OpResultCode.FAILURE;
                return emailResult;
            }

            emailResult.To = singleEmail.To;
            emailResult.Message = OpResultCode.SUCCCESS.ToString();
            emailResult.SubmittedAt = DateTime.Now;
            emailResult.UniqueEmailID = singleEmail.UniqueEmailID;
            emailResult.ErrorCode = OpResultCode.SUCCCESS;

            return emailResult;
        }
    }
}
