using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Transaction.Service.Account;

namespace Transaction.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly AkkaService _akkaService;

        public AccountController(AkkaService akkaService)
        {
            _akkaService = akkaService;
        }

        //[HttpPost]
        //public async Task<IActionResult> Post(string accountNumber, decimal amount, string currency)
        //{

        //    await _akkaService.OpenAccount(new OpenAccount(new AccountId(accountNumber), new Money(amount, new Currency(currency))));

        //    return Ok();
        //}

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] OpenAccountRequest request)
        {

            await _akkaService.OpenAccount(new OpenAccount(new AccountId(request.AccountNumber), new Money(request.Amount, new Currency(request.Currency))));

            return Ok();
        }
    }

    public record OpenAccountRequest(string AccountNumber, decimal Amount, string Currency);

}

