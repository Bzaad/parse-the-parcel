namespace ParcelShipping.Api.Contracts;

/// <summary>Returned when a valid parcel is too big or too heavy for every package.</summary>
public sealed record NoPackageResponse(string Message);
