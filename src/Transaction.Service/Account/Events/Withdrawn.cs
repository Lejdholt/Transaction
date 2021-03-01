namespace Transaction.Service.Account.Events
{
    public record Withdrawn(AccountId Id, TransactionId TransactionId, Money Amount) : IAccountEvent;
}