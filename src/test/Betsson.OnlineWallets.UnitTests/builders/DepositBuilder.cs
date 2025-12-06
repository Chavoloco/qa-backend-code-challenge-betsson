using Betsson.OnlineWallets.Models;

namespace Betsson.OnlineWallets.UnitTests.Builders
{
    public class DepositBuilder
    {
        private decimal _amount = 100m;

        public DepositBuilder WithAmount(decimal amount)
        {
            _amount = amount;
            return this;
        }

        public Deposit Build()
        {
            return new Deposit
            {
                Amount = _amount
            };
        }

        public static DepositBuilder ADeposit() => new();
    }
}