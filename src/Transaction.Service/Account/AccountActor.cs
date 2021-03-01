using Akka.Actor;
using Transaction.Service.Account.Commands;
using Transaction.Service.Account.Events;

namespace Transaction.Service.Account
{
    public class AccountActor : ObservableReceiveActor
    {
        private readonly IActorRef _store;
        private AccountId _id;
        private Money _funds;
        private TransactionId? _currentTransaction;
        private Money? _reservedFunds;

        public AccountActor(IActorRef store)
        {
            _store = store;


            Receive<OpenAccount>(openAccount =>
            {
                _funds = openAccount.Money;
                _id = openAccount.AccountId;

                Store(new AccountOpened(_id, _funds));
            });

            Receive<Reserve>(reserve =>
            {
                if (_currentTransaction != null)
                {

                }

                if (_funds!.Currency != reserve.Funds.Currency)
                {

                }

                if (_funds.Amount < reserve.Funds.Amount)
                {

                }

                _currentTransaction = reserve.Id;
                _reservedFunds = reserve.Funds;

                Store(new Reserved(_id, reserve.Id, _reservedFunds, reserve.To));
            });

            Receive<FinalizeWithdraw>(finalizeWithdraw =>
            {
                if (finalizeWithdraw.Id != _currentTransaction)
                {
                    return;
                }

                _funds = _funds with { Amount = _funds.Amount - _reservedFunds.Amount };

                Store(new Withdrawn(_id, finalizeWithdraw.Id, _reservedFunds));
            });

            Receive<Deposit>(deposit =>
            {
                if (_currentTransaction != null)
                {

                }

                if (_funds!.Currency != deposit.Funds.Currency)
                {

                }

                _funds = _funds with { Amount = _funds.Amount + deposit.Funds.Amount };

                Store(new Deposited(_id, deposit.Id, deposit.From, deposit.Funds));
            });

        }

        private void Store<T>(T @event) where T : IAccountEvent
        {
            _store.TellObservable(@event);
            Sender.TellObservable(@event);
        }
    }
}