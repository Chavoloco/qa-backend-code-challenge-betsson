using System;
using Betsson.OnlineWallets.Models;

namespace Betsson.OnlineWallets.UnitTests.Builders
{
    public class WithdrawalBuilder
    {
        private decimal _amount = 50m;

        public WithdrawalBuilder WithAmount(decimal amount)
        {
            _amount = amount;
            return this;
        }

        public Withdrawal Build()
        {
            return new Withdrawal
            {
                Amount = _amount
            };
        }

        public static WithdrawalBuilder AWithdrawal() => new();
    }
}
