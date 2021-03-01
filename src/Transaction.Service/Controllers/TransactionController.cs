using System;
using Microsoft.AspNetCore.Mvc;
using Transaction.Service.Account;

namespace Transaction.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly AkkaService _akkaService;

        public TransactionController(AkkaService akkaService)
        {
            _akkaService = akkaService;
        }

        //[HttpPost]
        //public IActionResult Post(string from, string to, decimal amount, string currency)
        //{
        //    var transactionId = new TransactionId(Guid.NewGuid());
        //    _akkaService.StartTransaction(new Transaction(transactionId, new AccountId(from), new AccountId(to), new Money(amount, new Currency(currency))));

        //    return Ok(transactionId.Id);
        //}

        [HttpPost]
        public IActionResult Post([FromBody] TransactionRequest request)
        {
            var transactionId = new TransactionId(request.TransactionId);
            _akkaService.StartTransaction(new Account.Transaction(transactionId, new AccountId(request.From), new AccountId(request.To), new Money(request.Amount, new Currency(request.Currency))));

            return Ok(transactionId.Id);
        }


    }
    public record TransactionRequest(Guid TransactionId, string From, string To, decimal Amount, string Currency);

}