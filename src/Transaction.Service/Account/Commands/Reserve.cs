namespace Transaction.Service.Account.Commands
{
    public record Reserve(TransactionId Id, Money Funds, AccountId To);
}