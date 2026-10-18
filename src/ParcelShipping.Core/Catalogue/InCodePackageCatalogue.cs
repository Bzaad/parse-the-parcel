namespace ParcelShipping.Core;

public sealed class InCodePackageCatalogue : IPackageCatalogue
{
    private static readonly IReadOnlyList<PackageDefinition> Packages =
    [
        new(PackageType.Small, Length: 200m, Breadth: 300m, Height: 150m, Cost: 5.00m),
        new(PackageType.Medium, Length: 300m, Breadth: 400m, Height: 200m, Cost: 7.50m),
        new(PackageType.Large, Length: 400m, Breadth: 600m, Height: 250m, Cost: 8.50m),
    ];

    public IReadOnlyList<PackageDefinition> GetPackages() => Packages;
}
