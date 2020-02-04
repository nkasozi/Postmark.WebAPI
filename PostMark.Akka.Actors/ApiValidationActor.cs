using Akka.Actor;
using Postmark.Shared;
using Postmark.Shared.Interfaces;
using Postmark.WebAPI.Models;
using Postmark.WebAPI.ValidationLogic;
using Postmark.WebAPI.ValidationLogic.Interfaces;
using System.Threading.Tasks;

namespace PostMark.Akka.Actors
{
    public class ApiValidationActor : BaseActor
    {
        private readonly IEmailValidationRulesEvaluator _rulesEvaluator;

        public ApiValidationActor(IEmailValidationRulesEvaluator rulesEvaluator)
        {
            _rulesEvaluator = rulesEvaluator;
        }

        public static Props Create(IEmailValidationRulesEvaluator rulesEvaluator)
        {
            return Props.Create(() => new ApiValidationActor(rulesEvaluator));
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case ValidateEmailRequestCmd request:
                    Task.Run(async () => await ProcessValidateEmailRequestCmd(request)).PipeTo(Sender);
                    break;

                //unhandled message sent
                default:
                    HandleDefault(message, GetType().Name);
                    break;
            }
        }

        private async Task<EmailResult> ProcessValidateEmailRequestCmd(ValidateEmailRequestCmd request)
        {
            var emailRequest = request.EmailRequest;
            var ruleEvalResult = _rulesEvaluator.RunAllRules(ref emailRequest);
            return ruleEvalResult;
        }
        public class ValidateEmailRequestCmd
        {
            public EmailRequest EmailRequest { get; set; }
        }
    }
}
