using Akka.Actor;
using Postmark.BussinessLogic.Interfaces;
using Postmark.WebAPI.Models;
using System;
using System.Threading.Tasks;
using static PostMark.Akka.Actors.EmailNotiificationActor;

namespace PostMark.Akka.Actors
{
    public class BussinessLogicActor : BaseActor
    {
        private readonly IBussinessRulesEvaluator _rulesEvaluator;
        private readonly IActorRef _emailSendingActor;
        private readonly IActorRef _failedEmailsActor;

        public BussinessLogicActor(IActorRef emailSendingActor, IActorRef failedEmailsActor, IBussinessRulesEvaluator rulesEvaluator)
        {
            _rulesEvaluator = rulesEvaluator;
            _emailSendingActor = emailSendingActor;
            _failedEmailsActor = failedEmailsActor;
        }

        public static Props Create(IActorRef emailSendingActor, IActorRef failedEmailsActor, IBussinessRulesEvaluator rulesEvaluator)
        {
            return Props.Create(() => new BussinessLogicActor(emailSendingActor, failedEmailsActor, rulesEvaluator));
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case ApplyBussinessRulesCmd request:
                    Task.Run(async () => await ProcessApplyBussinessRulesCmd(request)).PipeTo(Sender);
                    break;

                //unhandled message sent
                default:
                    HandleDefault(message, GetType().Name);
                    break;
            }
        }

        private async Task<EmailResult> ProcessApplyBussinessRulesCmd(ApplyBussinessRulesCmd request)
        {
            var emailRequest = request.EmailRequest;
            var rulesResult = _rulesEvaluator.RunAllRules(ref emailRequest);


            switch (rulesResult)
            {
                case SingleEmailResult singleEmailResult:
                    return ProcessSingleEmailResult(singleEmailResult, request);


                case BulkEmailResult bulkEmailResult:
                    return ProcessBulkEmailResult(bulkEmailResult, request);

            }

            return rulesResult;
        }

        private EmailResult ProcessBulkEmailResult(BulkEmailResult bulkEmailResult, ApplyBussinessRulesCmd request)
        {
            foreach (var singleEmailResult in bulkEmailResult.Results)
            {
                var email = singleEmailResult;
                ProcessSingleEmailResult(email, request);
            }

            return bulkEmailResult;
        }



        private SingleEmailResult ProcessSingleEmailResult(SingleEmailResult singleEmailResult, ApplyBussinessRulesCmd cmd)
        {
            singleEmailResult.MessageID = Guid.NewGuid().ToString();

            switch (singleEmailResult.ErrorCode)
            {
                case OpResultCode.SUCCCESS:
                    var sendEmailCmd = GetSendEmailCmd(cmd, singleEmailResult);
                    _emailSendingActor.Tell(sendEmailCmd);
                    break;

                case OpResultCode.FAILURE:
                    var logFailedEmailCmd = GetLogFailedEmailCmd(cmd, singleEmailResult);
                    _failedEmailsActor.Tell(logFailedEmailCmd);
                    break;
            }

            return singleEmailResult;
        }

        private NotifyByEmailCmd GetSendEmailCmd(ApplyBussinessRulesCmd request, SingleEmailResult singleEmailResult)
        {
            switch (request.EmailRequest)
            {
                case SingleEmail singleEmail:
                    return new NotifyByEmailCmd
                    {
                        EmailRequest = singleEmail
                    };

                default:
                    BulkEmail bulkEmail = request.EmailRequest as BulkEmail;
                    return new NotifyByEmailCmd
                    {
                        EmailRequest = bulkEmail.Emails.Find(i => i.UniqueEmailID == singleEmailResult.UniqueEmailID)
                    };
            }
        }

        private object GetLogFailedEmailCmd(ApplyBussinessRulesCmd request, SingleEmailResult singleEmailResult)
        {
            switch (request.EmailRequest)
            {
                case SingleEmail singleEmail:
                    return new PersistentStorageActor.LogFailedEmailCmd
                    {
                        EmailRequest = singleEmail,
                        FailureReason = singleEmailResult.Message
                    };

                default:
                    BulkEmail bulkEmail = request.EmailRequest as BulkEmail;
                    return new PersistentStorageActor.LogFailedEmailCmd
                    {
                        EmailRequest = bulkEmail.Emails.Find(i => i.UniqueEmailID == singleEmailResult.UniqueEmailID),
                        FailureReason = singleEmailResult.Message
                    };
            }
           
        }

        public class ApplyBussinessRulesCmd
        {
            public EmailRequest EmailRequest { get; set; }
        }
    }
}
