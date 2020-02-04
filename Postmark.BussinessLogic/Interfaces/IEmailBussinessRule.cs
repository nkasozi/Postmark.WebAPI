using Postmark.Shared.Interfaces;
using Postmark.WebAPI.Models;

namespace Postmark.BussinessLogic.Interfaces
{
    public interface IEmailBussinessRule: IRule<EmailRequest,EmailResult>
    {
    }
}
