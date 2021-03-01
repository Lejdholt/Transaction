namespace Transaction.Service.Account
{
    public record AccountOpened(AccountId Id, Money Funds) : IAccountEvent;
}