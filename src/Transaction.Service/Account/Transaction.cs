namespace Transaction.Service.Account
{
    public record Transaction(TransactionId Id, AccountId From, AccountId To, Money Funds);
}