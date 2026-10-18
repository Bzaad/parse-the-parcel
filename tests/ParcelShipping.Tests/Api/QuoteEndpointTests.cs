using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using ParcelShipping.Api.Controllers;

namespace ParcelShipping.Tests.Api;

public class QuoteEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const string Url = "/api/packages/quote";

    private readonly HttpClient _client;

    public QuoteEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_ParcelFits_Returns200WithPackageAndCost()
    {
        var response = await _client.PostAsJsonAsync(Url, new { length = 250, breadth = 350, height = 180, weight = 3.2 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(7.50m, body.GetProperty("cost").GetDecimal());
        Assert.Equal(1, body.GetProperty("packageType").GetInt32()); // PackageType.Medium
    }

    [Theory]
    [InlineData(700, 300, 150, 5)] // oversized
    [InlineData(100, 100, 100, 26)] // overweight
    public async Task Post_OversizedOrOverweight_Returns422WithMessage(
        decimal length, decimal breadth, decimal height, decimal weight)
    {
        var response = await _client.PostAsJsonAsync(Url, new { length, breadth, height, weight });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(PackagesController.NoPackageMessage, body.GetProperty("message").GetString());
    }

    [Theory]
    [InlineData(0, 300, 150, 5)]
    [InlineData(200, -1, 150, 5)]
    [InlineData(200, 300, 0, 5)]
    [InlineData(200, 300, 150, -2)]
    public async Task Post_ZeroOrNegativeValue_Returns400(decimal length, decimal breadth, decimal height, decimal weight)
    {
        var response = await _client.PostAsJsonAsync(Url, new { length, breadth, height, weight });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_NonNumericValue_Returns400()
    {
        var response = await _client.PostAsJsonAsync(Url, new { length = "abc", breadth = 300, height = 150, weight = 5 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_MissingField_Returns400()
    {
        var response = await _client.PostAsJsonAsync(Url, new { length = 200, breadth = 300, height = 150 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
