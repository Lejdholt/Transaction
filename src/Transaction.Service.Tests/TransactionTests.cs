using System;
using System.Threading.Tasks;
using Transaction.Service.Account;
using Transaction.Service.Account.Events;
using Transaction.Service.Controllers;
using Xunit;

namespace Transaction.Service.Tests
{
    public class TransactionTests : IntegrationTestBase
    {
        [Fact]
        public async Task GivenTwoExistingAccounts_WhenTransactionCoveredByFromAccount_ThenFundsShouldBeReserved()
        {
            var accountFrom = Guid.NewGuid().ToString();
            var accountTo = Guid.NewGuid().ToString();
            var transactionId = Guid.NewGuid();

            //Given
            await Post("/api/Account", new OpenAccountRequest(accountFrom, 100, "SEK"));
            await Post("/api/Account", new OpenAccountRequest(accountTo, 100, "SEK"));
            //When
            await Post("/api/transaction", new TransactionRequest(transactionId, accountFrom, accountTo, 50, "SEK"));

            //Then
            ExpectMessageOnKafka<Reserved>(new Reserved(
                new AccountId(accountFrom),
                new TransactionId(transactionId),
                new Money(50, new Currency("SEK")),
                new AccountId(accountTo)));
        }
    }
}
