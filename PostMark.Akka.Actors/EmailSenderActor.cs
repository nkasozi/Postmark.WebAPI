using Akka.Actor;
using Postmark.WebAPI.Models;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace PostMark.Akka.Actors
{
    public partial class EmailNotiificationActor : BaseActor
    {
        private readonly ISmtpClient _smtpClient;
        private readonly IActorRef _persistentStorageActor;

        public EmailNotiificationActor(ISmtpClient client, IActorRef PersistentStorageActor)
        {
            _smtpClient = client;
            _persistentStorageActor = PersistentStorageActor;
        }

        public static Props Create(ISmtpClient client, IActorRef failedEmailsActor, IActorRef PersistentStorageActor)
        {
            return Props.Create(() => new EmailNotiificationActor(client, PersistentStorageActor));
        }

        protected override void OnReceive(object message)
        {
            switch (message)
            {
                case NotifyByEmailCmd request:
                    Sender.Tell(ProcessSendEmailRequest(request));
                    break;

                default:
                    HandleDefault(message, GetType().Name);
                    break;
            }
        }

        private async Task<OpResult> ProcessSendEmailRequest(NotifyByEmailCmd cmd)
        {
            //sending will come later
            var sendResult = await SendEmail(cmd);

            switch (sendResult.ResultCode)
            {
                case OpResultCode.SUCCCESS:
                    {
                        var SaveSuccessfullEmailCmd = GetSaveCmd(sendResult, cmd);

                        _persistentStorageActor.Tell(SaveSuccessfullEmailCmd);

                        return new OpResult
                        {
                            ResultCode = OpResultCode.SUCCCESS,
                            StatusDesc = OpResultCode.SUCCCESS.ToString()
                        };
                    }

                default:
                    {
                        var logFailedEmailCmd = GetLogCmd(sendResult, cmd);

                        _persistentStorageActor.Tell(logFailedEmailCmd);

                        return new OpResult
                        {
                            ResultCode = OpResultCode.FAILURE,
                            StatusDesc = sendResult.StatusDesc
                        };
                    }

            }
        }

        private PersistentStorageActor.LogFailedEmailCmd GetLogCmd(OpResult sendResult, NotifyByEmailCmd request)
        {
            return new PersistentStorageActor.LogFailedEmailCmd
            {
                EmailRequest = request.EmailRequest,
                FailureReason = sendResult.StatusDesc
            };
        }

        private PersistentStorageActor.SaveSentEmailCmd GetSaveCmd(OpResult sendResult, NotifyByEmailCmd cmd)
        {
            return new PersistentStorageActor.SaveSentEmailCmd
            {
                EmailRequest = cmd.EmailRequest
            };

        }

        private async Task<OpResult> SendEmail(NotifyByEmailCmd request)
        {
            return await Task.Run(() =>
            {
                return new OpResult
                {
                    ResultCode = OpResultCode.SUCCCESS,
                    StatusDesc = OpResultCode.SUCCCESS.ToString()
                };
            });
        }



        public class NotifyByEmailCmd
        {
            public EmailRequest EmailRequest { get; set; }
        }

        public class SmtpClient : System.Net.Mail.SmtpClient, ISmtpClient
        {

        }

        public interface ISmtpClient
        {
            string Host { get; set; }
            int Port { get; set; }
            bool EnableSsl { get; set; }
            SmtpDeliveryMethod DeliveryMethod { get; set; }
            ICredentialsByHost Credentials { get; set; }

            void Send(MailMessage message);
        }
    }
}
