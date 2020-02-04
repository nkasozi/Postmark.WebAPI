using Postmark.BussinessLogic.Interfaces;
using Postmark.Shared;
using Postmark.Shared.Interfaces;
using Postmark.WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Postmark.BussinessLogic
{
    public class UnsubscribeRule : IEmailBussinessRule
    {
        public List<string> UnbsubscribedEmails = new List<string>();

        public const string USER_ALREADY_UNSUBSCRIBED_MESSAGE = "User is Unsubscribed";
        public PreconditionResult RunPreConditionChecks(ref EmailRequest obj)
        {
            //every item should be run against the rule
            return new PreconditionResult
            {
                ResultCode = PreconditionResultCode.SUCCESS,
                ResultDesc = PreconditionResultCode.SUCCESS.ToString()
            };
        }

        public EmailResult RunRuleChecks(ref EmailRequest obj)
        {
            EmailResult ruleEvaluationResult = null;

            switch (obj)
            {
                case SingleEmail singleEmail:
                    ruleEvaluationResult = GenerateSingleEmailResult(singleEmail);
                    break;

                case BulkEmail bulkEmail:
                    ruleEvaluationResult = GenerateBulkEmailResult(bulkEmail);
                    break;

            }

            return ruleEvaluationResult;
        }

        private SingleEmailResult GenerateSingleEmailResult(SingleEmail singleEmail)
        {
            //IRuleEvaluationResult<EmailResult> ruleEvaluationResult = new EmailRuleEvaluationResult();

            var singleEmailResult = checkIfAlreadyUnsubcribed(singleEmail);

            return singleEmailResult;

        }

        private BulkEmailResult GenerateBulkEmailResult(BulkEmail bulkEmail)
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
                emailResult.Message = USER_ALREADY_UNSUBSCRIBED_MESSAGE;
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
