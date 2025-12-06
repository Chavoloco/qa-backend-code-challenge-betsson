using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Betsson.OnlineWallets.IntegrationTests.Fixtures;
using Betsson.OnlineWallets.Web.Models;
using FluentAssertions;
using Xunit;

namespace Betsson.OnlineWallets.IntegrationTests.Controllers
{
    public class OnlineWalletControllerTests : IClassFixture<WebApplicationFixture>
    {
        private readonly WebApplicationFixture _fixture;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public OnlineWalletControllerTests(WebApplicationFixture fixture)
        {
            _fixture = fixture;
        }

        #region TC4 - GET /OnlineWallet/Balance Tests

        [Fact]
        [Trait("Endpoint", "Balance")]
        [Trait("Method", "GET")]
        public async Task Balance_GET_ReturnsOkWithBalance()
        {
            var client = _fixture.Client;

            var response = await client!.GetAsync("/OnlineWallet/Balance");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType?.MediaType.Should().Contain("application/json");

            var content = await response.Content.ReadAsStringAsync();
            var balanceResponse = JsonSerializer.Deserialize<BalanceResponse>(content, _jsonOptions);
            balanceResponse.Should().NotBeNull();
            balanceResponse!.Amount.Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        [Trait("Endpoint", "Balance")]
        [Trait("Method", "GET")]
        public async Task Balance_GET_ResponseContainsAmountField()
        {
            var client = _fixture.Client;

            var response = await client!.GetAsync("/OnlineWallet/Balance");
            var content = await response.Content.ReadAsStringAsync();

            content.Should().Contain("\"amount\"");
        }

        #endregion
        #region POST /OnlineWallet/Deposit Tests

        [Fact]
        [Trait("Endpoint", "Deposit")]
        [Trait("Method", "POST")]
        [Trait("Category", "Happy Path")]
        public async Task Deposit_POST_WithValidAmount_ReturnsOkWithUpdatedBalance()
        {
            var client = _fixture.Client;
            var depositRequest = new DepositRequest { Amount = 100m };
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(depositRequest),
                Encoding.UTF8,
                "application/json");

            var response = await client!.PostAsync("/OnlineWallet/Deposit", jsonContent);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var balanceResponse = JsonSerializer.Deserialize<BalanceResponse>(content, _jsonOptions);
            balanceResponse.Should().NotBeNull();
            balanceResponse!.Amount.Should().BeGreaterThan(0);
        }

        [Fact]
        [Trait("Endpoint", "Deposit")]
        [Trait("Method", "POST")]
        [Trait("Category", "Happy Path")]
        public async Task Deposit_POST_WithMultipleDeposits_IncreasesBalance()
        {
            var client = _fixture.Client;

            var deposit1 = new DepositRequest { Amount = 50m };
            var content1 = new StringContent(
                JsonSerializer.Serialize(deposit1),
                Encoding.UTF8,
                "application/json");
            var response1 = await client!.PostAsync("/OnlineWallet/Deposit", content1);
            var result1 = await response1.Content.ReadAsStringAsync();
            var balance1 = JsonSerializer.Deserialize<BalanceResponse>(result1, _jsonOptions);

            var deposit2 = new DepositRequest { Amount = 75m };
            var content2 = new StringContent(
                JsonSerializer.Serialize(deposit2),
                Encoding.UTF8,
                "application/json");
            var response2 = await client!.PostAsync("/OnlineWallet/Deposit", content2);
            var result2 = await response2.Content.ReadAsStringAsync();
            var balance2 = JsonSerializer.Deserialize<BalanceResponse>(result2, _jsonOptions);

            response1.StatusCode.Should().Be(HttpStatusCode.OK);
            response2.StatusCode.Should().Be(HttpStatusCode.OK);
            balance2!.Amount.Should().BeGreaterThan(balance1!.Amount);
        }

        [Fact]
        [Trait("Endpoint", "Deposit")]
        [Trait("Method", "POST")]
        [Trait("Category", "Edge Case")]
        public async Task Deposit_POST_WithZeroAmount_ShouldHandleGracefully()
        {
            var client = _fixture.Client;
            var depositRequest = new DepositRequest { Amount = 0m };
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(depositRequest),
                Encoding.UTF8,
                "application/json");

            var response = await client!.PostAsync("/OnlineWallet/Deposit", jsonContent);

            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("Endpoint", "Deposit")]
        [Trait("Method", "POST")]
        [Trait("Category", "Edge Case")]
        public async Task Deposit_POST_WithLargeAmount_ReturnsOk()
        {
            var client = _fixture.Client;
            var depositRequest = new DepositRequest { Amount = 999999.99m };
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(depositRequest),
                Encoding.UTF8,
                "application/json");

            var response = await client!.PostAsync("/OnlineWallet/Deposit", jsonContent);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        #endregion
        #region POST /OnlineWallet/Withdraw Tests

        [Fact]
        [Trait("Endpoint", "Withdraw")]
        [Trait("Method", "POST")]
        [Trait("Category", "Happy Path")]
        public async Task Withdraw_POST_WithSufficientBalance_ReturnsOk()
        {
            var client = _fixture.Client;

            var deposit = new DepositRequest { Amount = 100m };
            var depositContent = new StringContent(
                JsonSerializer.Serialize(deposit),
                Encoding.UTF8,
                "application/json");
            await client!.PostAsync("/OnlineWallet/Deposit", depositContent);

            var withdrawalRequest = new WithdrawalRequest { Amount = 50m };
            var withdrawalContent = new StringContent(
                JsonSerializer.Serialize(withdrawalRequest),
                Encoding.UTF8,
                "application/json");

            var response = await client!.PostAsync("/OnlineWallet/Withdraw", withdrawalContent);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var balanceResponse = JsonSerializer.Deserialize<BalanceResponse>(content, _jsonOptions);
            balanceResponse.Should().NotBeNull();
            balanceResponse!.Amount.Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        [Trait("Endpoint", "Withdraw")]
        [Trait("Method", "POST")]
        [Trait("Category", "Error")]
        public async Task Withdraw_POST_WithInsufficientBalance_ReturnsBadRequest()
        {
            var client = _fixture.Client;
            var withdrawalRequest = new WithdrawalRequest { Amount = 10000m };
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(withdrawalRequest),
                Encoding.UTF8,
                "application/json");

            var response = await client!.PostAsync("/OnlineWallet/Withdraw", jsonContent);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("Endpoint", "Withdraw")]
        [Trait("Method", "POST")]
        [Trait("Category", "Happy Path")]
        public async Task Withdraw_POST_ExactBalance_DecreasesToZero()
        {
            var client = _fixture.Client;

            var balanceResponse = await client!.GetAsync("/OnlineWallet/Balance");

            var balanceContent = await balanceResponse.Content.ReadAsStringAsync();
            var balanceAmount = JsonSerializer.Deserialize<BalanceResponse>(balanceContent, _jsonOptions);
            var exactAmount = balanceAmount!.Amount;

            if (exactAmount == 0)
            {
                var depositForExact = new DepositRequest { Amount = 100m };
                var depositContentForExact = new StringContent(
                    JsonSerializer.Serialize(depositForExact),
                    Encoding.UTF8,
                    "application/json");
                await client.PostAsync("/OnlineWallet/Deposit", depositContentForExact);
                exactAmount = 100m;
            }

            var withdrawal = new WithdrawalRequest { Amount = exactAmount };
            var withdrawalContent = new StringContent(
                JsonSerializer.Serialize(withdrawal),
                Encoding.UTF8,
                "application/json");

            var response = await client!.PostAsync("/OnlineWallet/Withdraw", withdrawalContent);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            var finalBalanceResponse = JsonSerializer.Deserialize<BalanceResponse>(content, _jsonOptions);
            finalBalanceResponse!.Amount.Should().Be(0);
        }

        #endregion
    }
}