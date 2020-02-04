using Akka.Actor;
using Postmark.WebAPI.Models;
using System;
using System.Threading.Tasks;

namespace PostMark.Akka.Actors
{
    public class ApiListenerActor : BaseActor
    {
        private readonly IActorRef _apiValidationActor;
        private readonly IActorRef _bussinessLogicActor;

        public ApiListenerActor(IActorRef apiValidationActor, IActorRef BussinessLogicActor)
        {
            _apiValidationActor = apiValidationActor;
            _bussinessLogicActor = BussinessLogicActor;
        }
        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case SingleEmail request:
                    Task.Run(async () => await ProcessEmailRequest(request)).PipeTo(Sender);
                    break;

                case BulkEmail request:
                    Task.Run(async () => await ProcessEmailRequest(request)).PipeTo(Sender);
                    break;

                //unhandled message sent
                default:
                    HandleDefault(message, GetType().Name);
                    break;
            }
        }
        private async Task<OpResult> ProcessEmailRequest(EmailRequest request)
        {
            var validateCmd = new ApiValidationActor.ValidateEmailRequestCmd
            {
                EmailRequest = request
            };

            var validationResult = await _apiValidationActor.Ask<OpResult>(validateCmd);

            if (validationResult.ResultCode != OpResultCode.SUCCCESS)
            {
                return validationResult;
            }

            var sendBasedOnBussinessRules = new BussinessLogicActor.ApplyBussinessRulesCmd
            {
                EmailRequest = request
            };

            var bussinessRulesResult = await _bussinessLogicActor.Ask<OpResult>(sendBasedOnBussinessRules);

            return bussinessRulesResult;
        }
    }
}
