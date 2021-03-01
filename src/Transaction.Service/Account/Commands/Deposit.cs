namespace Transaction.Service.Account.Commands
{
    public record Deposit(TransactionId Id, Money Funds, AccountId From);
}