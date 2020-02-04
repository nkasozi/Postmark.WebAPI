using Postmark.Shared.Interfaces;
using Postmark.WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Postmark.WebAPI.ValidationLogic.Interfaces
{
    public interface IValidationRule : IRule<EmailRequest, EmailResult>
    {
    }
}
