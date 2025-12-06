using System;
using Betsson.OnlineWallets.Data.Repositories;
using Moq;

namespace Betsson.OnlineWallets.UnitTests.Fixtures
{
    public class OnlineWalletFixture : IDisposable
    {
        public Mock<IOnlineWalletRepository> MockRepository { get; }

        public OnlineWalletFixture()
        {
            MockRepository = new Mock<IOnlineWalletRepository>();
        }

        public void Dispose()
        {
            MockRepository.Reset();
        }

        public static OnlineWalletFixture Create() => new();
    }
}