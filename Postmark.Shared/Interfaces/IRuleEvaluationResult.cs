using Postmark.Shared;

namespace Postmark.Shared.Interfaces
{
    public interface IRuleEvaluationResult<T>
    {
        RulesResultCode ResultCode { get; set; }
        string ResultDesc { get; set; }
        T Result { get; set; }
    }
}