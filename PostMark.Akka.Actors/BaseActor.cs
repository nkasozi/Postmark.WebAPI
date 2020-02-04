using Akka.Actor;
using Postmark.WebAPI.Models;

namespace PostMark.Akka.Actors
{
    public abstract class BaseActor : UntypedActor
    {

        protected void HandleDefault(object message, string actorType)
        {
            switch (message)
            {

                default:
                    Sender.Tell
                    (
                        new OpResult
                        {
                            ResultCode = OpResultCode.FAILURE,
                            StatusDesc = $"ERROR: MESSAGE OF TYPE [{message.GetType().Name}] CANT BE HANDLED BY ACTOR OF TYPE [{actorType}]"
                        }
                    );
                    break;
            }
        }
    }
}