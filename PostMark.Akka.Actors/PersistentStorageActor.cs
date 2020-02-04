using Akka.Actor;
using Postmark.PersistentStorage.Interface;
using Postmark.WebAPI.Models;
using System.Threading.Tasks;

namespace PostMark.Akka.Actors
{
    public class PersistentStorageActor : BaseActor
    {
        private readonly IPersistentStorage _persistentStorage;
       
        public PersistentStorageActor(IPersistentStorage persistentStorage)
        {
            _persistentStorage = persistentStorage;
        }

        public static Props Create(IPersistentStorage persistentStorage)
        {
            return Props.Create(() => new PersistentStorageActor(persistentStorage));
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {

                case SaveSentEmailCmd request:
                    Task.Run(async () => await ProcessSaveSentEmailCmd(request)).PipeTo(Sender);
                    break;

                case LogFailedEmailCmd request:
                    Task.Run(async () => await ProcessLogFailedEmailCmd(request)).PipeTo(Sender);
                    break;

                //unhandled message sent
                default:
                    HandleDefault(message, GetType().Name);
                    break;
            }
        }

        private async Task<OpResult> ProcessSaveSentEmailCmd(SaveSentEmailCmd request)
        {
            //TODO: save in DB
            return new OpResult
            {
                ResultCode = OpResultCode.SUCCCESS,
                StatusDesc = OpResultCode.SUCCCESS.ToString()
            };
        }

        private async Task<OpResult> ProcessLogFailedEmailCmd(LogFailedEmailCmd request)
        {
            //TODO: save in DB
            return new OpResult
            {
                ResultCode = OpResultCode.SUCCCESS,
                StatusDesc = OpResultCode.SUCCCESS.ToString()
            };
        }

        public class SaveSentEmailCmd
        {
            public EmailRequest EmailRequest { get; set; }
        }

        public class LogFailedEmailCmd
        {
            public EmailRequest EmailRequest { get; set; }
            public string FailureReason { get; set; }
        }
    }
}
