using System;
using System.Threading.Tasks;
using Betsson.OnlineWallets.Exceptions;
using Betsson.OnlineWallets.Services;
using Betsson.OnlineWallets.UnitTests.Builders;
using Betsson.OnlineWallets.UnitTests.Fixtures;
using FluentAssertions;
using Moq;
using Xunit;

namespace Betsson.OnlineWallets.UnitTests.Services
{
    public class OnlineWalletsServiceTest
    {
        #region DepositFundsAsync Tests

        [Fact]
        public async Task DepositFundsAsync_WithValidDeposit_IncreasesBalance()
        {
            var fixture = OnlineWalletFixture.Create();
            var currentEntry = OnlineWalletEntryBuilder.AnOnlineWalletEntry()
                .WithBalanceBefore(100)
                .WithAmount(50)
                .Build();

            fixture.MockRepository
                .Setup(r => r.GetLastOnlineWalletEntryAsync())
                .ReturnsAsync(currentEntry);

            fixture.MockRepository
                .Setup(r => r.InsertOnlineWalletEntryAsync(It.IsAny<Betsson.OnlineWallets.Data.Models.OnlineWalletEntry>()))
                .Returns(Task.CompletedTask);

            var service = new OnlineWalletService(fixture.MockRepository.Object);
            var deposit = DepositBuilder.ADeposit().WithAmount(25).Build();

            // Act
            var newBalance = await service.DepositFundsAsync(deposit);

            // Assert
            newBalance.Amount.Should().Be(175);
            fixture.MockRepository.Verify(
                r => r.InsertOnlineWalletEntryAsync(It.Is<Betsson.OnlineWallets.Data.Models.OnlineWalletEntry>(
                    e => e.Amount == 25 && e.BalanceBefore == 150)),
                Times.Once);
        }

        [Fact]
        public async Task DepositFundsAsync_WithZeroBalance_CreatesFirstTransaction()
        {
            // Arrange
            var fixture = OnlineWalletFixture.Create();
            fixture.MockRepository
                .Setup(r => r.GetLastOnlineWalletEntryAsync())
                .ReturnsAsync((Betsson.OnlineWallets.Data.Models.OnlineWalletEntry?)null);

            fixture.MockRepository
                .Setup(r => r.InsertOnlineWalletEntryAsync(It.IsAny<Betsson.OnlineWallets.Data.Models.OnlineWalletEntry>()))
                .Returns(Task.CompletedTask);

            var service = new OnlineWalletService(fixture.MockRepository.Object);
            var deposit = DepositBuilder.ADeposit().WithAmount(100).Build();

            // Act
            var newBalance = await service.DepositFundsAsync(deposit);

            // Assert
            newBalance.Amount.Should().Be(100);
            fixture.MockRepository.Verify(
                r => r.InsertOnlineWalletEntryAsync(It.Is<Betsson.OnlineWallets.Data.Models.OnlineWalletEntry>(
                    e => e.Amount == 100 && e.BalanceBefore == 0)),
                Times.Once);
        }

        [Fact]
        public async Task DepositFundsAsync_WithLargeAmount_HandlesCorrectly()
        {
            var fixture = OnlineWalletFixture.Create();
            var currentEntry = OnlineWalletEntryBuilder.AnOnlineWalletEntry()
                .WithBalanceBefore(0)
                .WithAmount(0)
                .Build();

            fixture.MockRepository
                .Setup(r => r.GetLastOnlineWalletEntryAsync())
                .ReturnsAsync(currentEntry);

            fixture.MockRepository
                .Setup(r => r.InsertOnlineWalletEntryAsync(It.IsAny<Betsson.OnlineWallets.Data.Models.OnlineWalletEntry>()))
                .Returns(Task.CompletedTask);

            var service = new OnlineWalletService(fixture.MockRepository.Object);
            var deposit = DepositBuilder.ADeposit().WithAmount(999999.99m).Build();

            var newBalance = await service.DepositFundsAsync(deposit);

            newBalance.Amount.Should().Be(999999.99m);
        }

        #endregion
    }
}