using Akka.Actor;
using Transaction.Service.Account.Commands;
using Transaction.Service.Account.Events;

namespace Transaction.Service.Account
{
    public class TransactionActor : ObservableReceiveActor
    {
        private IActorRef _from;
        private IActorRef _to;
        private TransactionId _transactionId;
        private Money _transactionFunds;
        private AccountId _transactionFrom;
        private AccountId _transactionTo;

        public TransactionActor(IActorRef accounting)
        {
            ReceiveAsync<Transaction>(async transaction =>
            {
                _transactionFrom = transaction.From;
                _from = await accounting.AskObservable<IActorRef,AccountId>(_transactionFrom);
                _transactionTo = transaction.To;
                _to = await accounting.AskObservable<IActorRef, AccountId>(_transactionTo);
                _transactionId = transaction.Id;
                _transactionFunds = transaction.Funds;

                _from.TellObservable(new Reserve(_transactionId, _transactionFunds, _transactionTo));

            });

            Receive<Reserved>(_ =>
           {
               _to.TellObservable(new Deposit(_transactionId, _transactionFunds, _transactionFrom));

           }, msg => msg.TransactionId.Equals(_transactionId));

            Receive<Deposited>(_ =>
           {
               _from.TellObservable(new FinalizeWithdraw(_transactionId));

           }, msg => msg.TransactionId.Equals(_transactionId));


            Receive<Withdrawn>(_ =>
           {
               Context.Stop(Self);

           }, msg => msg.TransactionId.Equals(_transactionId));
        }
    }
}