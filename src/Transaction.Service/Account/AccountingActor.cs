using System.Collections.Generic;
using Akka.Actor;

namespace Transaction.Service.Account
{
    public class AccountingActor : ObservableReceiveActor
    {

        private readonly Dictionary<AccountId, IActorRef> _accounts = new();
        private readonly IActorRef _store;

        public AccountingActor(IActorRef store)
        {
            _store = store;

            Receive<OpenAccount>(openAccount =>
            {
                var account = Context.ActorOf(Props.Create(() => new AccountActor(_store)), $"account_{openAccount.AccountId.Id}");
                account.TellObservable(openAccount);
                _accounts.Add(openAccount.AccountId, account);
                Sender.TellObservable("Opened");
            });


            Receive<AccountId>(account =>
            {
                Sender.TellObservable(_accounts[account]);
            });
        }
    }
}