using System;

namespace Transaction.Service.Account
{
    public record Currency
    {

        public Currency(string symbol)
        {
            if (symbol.Length != 3)
            {
                throw new Exception("Currency must be 3 characters");
            }
            this.Symbol = symbol.ToUpper();
        }
        public string Symbol { get; } 
    }
}