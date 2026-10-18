using ParcelShipping.Core;

namespace ParcelShipping.Api.Contracts;

/// <summary>The package chosen for a parcel and what it costs.</summary>
public sealed record QuoteResponse(PackageType PackageType, decimal Cost);
