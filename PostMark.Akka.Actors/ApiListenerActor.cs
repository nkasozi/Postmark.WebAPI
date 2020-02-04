using Akka.Actor;
using Postmark.WebAPI.Models;
using System.Threading.Tasks;

namespace PostMark.Akka.Actors
{
    public class ApiListenerActor : BaseActor
    {
        private readonly IActorRef _bussinessLogicActor;

        public ApiListenerActor(IActorRef BussinessLogicActor)
        {
            _bussinessLogicActor = BussinessLogicActor;
        }

        public static Props Create(IActorRef BussinessLogicActor)
        {
            return Props.Create(() => new ApiListenerActor(BussinessLogicActor));
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
        private async Task<EmailResult> ProcessEmailRequest(EmailRequest request)
        {
            var sendBasedOnBussinessRules = new BussinessLogicActor.ApplyBussinessRulesCmd
            {
                EmailRequest = request
            };

            var bussinessRulesResult = await _bussinessLogicActor.Ask<EmailResult>(sendBasedOnBussinessRules);

            return bussinessRulesResult;
        }
    }
}
