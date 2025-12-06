using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Betsson.OnlineWallets.Web;
using Xunit;

namespace Betsson.OnlineWallets.IntegrationTests.Fixtures
{
    public class WebApplicationFixture : IAsyncLifetime
    {
        private WebApplicationFactory<Program>? _factory;
        public HttpClient? Client { get; private set; }

        public Task InitializeAsync()
        {
            _factory = new WebApplicationFactory<Program>();
            Client = _factory.CreateClient();
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            Client?.Dispose();
            _factory?.Dispose();
            return Task.CompletedTask;
        }
    }
}