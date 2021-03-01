namespace Transaction.Service.Account.Events
{
    public record Reserved(AccountId Id, TransactionId TransactionId, Money Amount, AccountId To): IAccountEvent;
}