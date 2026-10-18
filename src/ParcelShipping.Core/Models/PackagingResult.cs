namespace ParcelShipping.Core;

/// <summary>The package type and cost for a parcel. "No solution" is represented by a null result, not by this type.</summary>
public sealed record PackagingResult(PackageType PackageType, decimal Cost);
