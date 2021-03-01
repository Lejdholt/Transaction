namespace Transaction.Service.Account.Events
{
    public record Deposited(AccountId Id,TransactionId TransactionId, AccountId From, Money Amount) : IAccountEvent;
}