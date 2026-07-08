using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;

namespace RiskAnalyzer.Api.Tests
{
    public class ApiStartupTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ApiStartupTests(WebApplicationFactory<Program> factory) => _factory = factory;

        [Fact]
        public void Factory_CreatesClient()
        {
            var client = _factory.CreateClient();
            Assert.NotNull(client);
        }
    }
}
