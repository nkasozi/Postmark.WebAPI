using Postmark.BussinessLogic.Interfaces;
using Postmark.Shared;
using Postmark.WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Postmark.BussinessLogic
{
    public class ValidateToEmailAddressRule : IEmailBussinessRule
    {
        public const string ERROR_MESSAGE = "The To Email is not Valid";

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
            
            var singleEmailResult = checkTheToEmailIsValid(singleEmail);

            return singleEmailResult;

        }

        private BulkEmailResult GenerateBulkEmailResult(BulkEmail bulkEmail)
        {
            BulkEmailResult bulkEmailResult = new BulkEmailResult();

            foreach (var singleEmail in bulkEmail.Emails)
            {
                var singleEmailResult = checkTheToEmailIsValid(singleEmail);

                bulkEmailResult.Results.Add(singleEmailResult);

            }

            return bulkEmailResult;

        }

        private SingleEmailResult checkTheToEmailIsValid(SingleEmail singleEmail)
        {
            var emailResult = new SingleEmailResult();
            
           
            if (!IsValidEmail(singleEmail.To))
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

        bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
