using Postmark.Shared.Interfaces;
using Postmark.WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Postmark.BussinessLogic.Interfaces
{
    public interface IEmailBussinessRule: IRule<EmailRequest,EmailResult>
    {
    }
}
