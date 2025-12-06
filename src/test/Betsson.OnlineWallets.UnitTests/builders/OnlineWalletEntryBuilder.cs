using System;
using Betsson.OnlineWallets.Data.Models;

namespace Betsson.OnlineWallets.UnitTests.Builders
{
    public class OnlineWalletEntryBuilder
    {
        private decimal _amount = 100m;
        private decimal _balanceBefore = 0m;
        private DateTimeOffset _eventTime = DateTimeOffset.UtcNow;

        public OnlineWalletEntryBuilder WithAmount(decimal amount)
        {
            _amount = amount;
            return this;
        }

        public OnlineWalletEntryBuilder WithBalanceBefore(decimal balanceBefore)
        {
            _balanceBefore = balanceBefore;
            return this;
        }

        public OnlineWalletEntryBuilder WithEventTime(DateTimeOffset eventTime)
        {
            _eventTime = eventTime;
            return this;
        }

        public OnlineWalletEntry Build()
        {
            return new OnlineWalletEntry
            {
                Amount = _amount,
                BalanceBefore = _balanceBefore,
                EventTime = _eventTime
            };
        }

        public static OnlineWalletEntryBuilder AnOnlineWalletEntry() => new();
    }
}
