using Microsoft.AspNetCore.Mvc;
using ParcelShipping.Api.Contracts;
using ParcelShipping.Core;

namespace ParcelShipping.Api.Controllers;

[ApiController]
[Route("api/packages")]
public sealed class PackagesController : ControllerBase
{
    public const string NoPackageMessage = "No package is available for a parcel of this size or weight.";

    private readonly PackageSelector _selector;

    public PackagesController(PackageSelector selector)
    {
        _selector = selector;
    }

    /// <summary>Finds the cheapest package that fits the parcel.</summary>
    [HttpPost("quote")]
    [ProducesResponseType<QuoteResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<NoPackageResponse>(StatusCodes.Status422UnprocessableEntity)]
    public IActionResult Quote(QuoteRequest request)
    {
        Parcel parcel;
        try
        {
            parcel = new Parcel(request.Length, request.Breadth, request.Height, request.Weight);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            var field = ex.ParamName ?? string.Empty;
            var errors = new Dictionary<string, string[]> { [field] = [$"{field} must be greater than zero."] };
            return BadRequest(new ValidationProblemDetails(errors)
            {
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
            });
        }

        var result = _selector.Select(parcel);

        return result is null
            ? UnprocessableEntity(new NoPackageResponse(NoPackageMessage))
            : Ok(new QuoteResponse(result.PackageType, result.Cost));
    }
}
