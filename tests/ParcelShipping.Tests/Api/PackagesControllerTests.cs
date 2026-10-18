using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ParcelShipping.Api.Contracts;
using ParcelShipping.Api.Controllers;
using ParcelShipping.Core;

namespace ParcelShipping.Tests.Api;

public class PackagesControllerTests
{
    private readonly PackagesController _controller = new(new PackageSelector(new InCodePackageCatalogue()));

    [Theory]
    [InlineData(200, 300, 150, 1, PackageType.Small, 5.00)]
    [InlineData(300, 400, 200, 10, PackageType.Medium, 7.50)]
    [InlineData(400, 600, 250, 25, PackageType.Large, 8.50)]
    public void Quote_ParcelFits_ReturnsOkWithPackageAndCost(
        decimal length, decimal breadth, decimal height, decimal weight, PackageType expectedType, decimal expectedCost)
    {
        var result = _controller.Quote(new QuoteRequest(length, breadth, height, weight));

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
        Assert.Equal(new QuoteResponse(expectedType, expectedCost), ok.Value);
    }

    [Theory]
    [InlineData(401, 600, 250, 5)] // oversized
    [InlineData(400, 601, 250, 5)]
    [InlineData(400, 600, 251, 5)]
    [InlineData(100, 100, 100, 25.01)] // overweight
    public void Quote_OversizedOrOverweight_ReturnsUnprocessableEntityWithMessage(
        decimal length, decimal breadth, decimal height, decimal weight)
    {
        var result = _controller.Quote(new QuoteRequest(length, breadth, height, weight));

        var unprocessable = Assert.IsType<UnprocessableEntityObjectResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, unprocessable.StatusCode);
        Assert.Equal(PackagesController.NoPackageMessage, Assert.IsType<NoPackageResponse>(unprocessable.Value).Message);
    }

    [Theory]
    [InlineData("Length", 0, 200, 300, 4.5)]
    [InlineData("Length", -1, 200, 300, 4.5)]
    [InlineData("Breadth", 100, 0, 300, 4.5)]
    [InlineData("Breadth", 100, -1, 300, 4.5)]
    [InlineData("Height", 100, 200, 0, 4.5)]
    [InlineData("Height", 100, 200, -1, 4.5)]
    [InlineData("Weight", 100, 200, 300, 0)]
    [InlineData("Weight", 100, 200, 300, -1)]
    public void Quote_ZeroOrNegativeValue_ReturnsBadRequestNamingTheField(
        string field, decimal length, decimal breadth, decimal height, decimal weight)
    {
        // Parcel names its arguments in lower case, so compare ignoring case.
        var result = _controller.Quote(new QuoteRequest(length, breadth, height, weight));

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
        var problem = Assert.IsType<ValidationProblemDetails>(badRequest.Value);
        var error = Assert.Single(problem.Errors);
        Assert.Equal(field, error.Key, ignoreCase: true);
        Assert.Equal($"{error.Key} must be greater than zero.", Assert.Single(error.Value));
    }
}
