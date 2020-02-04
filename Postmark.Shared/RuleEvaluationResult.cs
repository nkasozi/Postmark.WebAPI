using Postmark.Shared.Interfaces;
using Postmark.WebAPI.Models;

namespace Postmark.Shared
{
    public enum RulesResultCode
    {
        SUCCESS = 0,
        FAILED = 100
    }

    public static class RulesResultDesc
    {
        public const string SUCCESS = "SUCCESS";
        public const string NO_RULES_RUN = "NO_RULES_RUN";
    }

    public class RuleEvaluationResult<T> : IRuleEvaluationResult<T>
    {
        public RulesResultCode ResultCode { get; set; }
        public string ResultDesc { get; set; }
        public T Result { get; set; }
    }

    public class EmailRuleEvaluationResult : IRuleEvaluationResult<EmailResult>
    {
        public RulesResultCode ResultCode { get; set; }
        public string ResultDesc { get; set; }
        public EmailResult Result { get; set; }
    }
}
