using Akka.Actor;


namespace Postmark.WebAPI
{
    public delegate IActorRef ApiListenerActorProvider();

    public delegate IActorRef BussinessLogicActorProvider();

    public delegate IActorRef EmailSenderActorProvider();

    public delegate IActorRef PersistentStorageActorProvider();

}
