using Postmark.Shared.Interfaces;
using Postmark.WebAPI.Models;

namespace Postmark.BussinessLogic.Interfaces
{
    public interface IBussinessRulesEvaluator : IRulesEvaluator<IBussinessRulesEvaluator, IEmailBussinessRule, EmailRequest,EmailResult>
    {

    }
}