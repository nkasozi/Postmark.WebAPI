using Postmark.Shared;
using Postmark.Shared.Interfaces;
using Postmark.WebAPI.Models;

namespace Postmark.WebAPI.ValidationLogic.Interfaces
{
    public interface IEmailValidationRulesEvaluator : IRulesEvaluator<IEmailValidationRulesEvaluator, IValidationRule, EmailRequest, EmailResult>
    {
    }
}