namespace ParcelShipping.Core;

public sealed class PackageSelector
{
    private const decimal MaxWeightKg = 25m;

    private readonly IPackageCatalogue _catalogue;

    public PackageSelector() : this(new InCodePackageCatalogue())
    {
    }

    public PackageSelector(IPackageCatalogue catalogue)
    {
        _catalogue = catalogue;
    }

    /// <summary>Returns the cheapest package that fits the parcel, or null when there is no solution.</summary>
    public PackagingResult? Select(Parcel parcel)
    {
        if (parcel.Weight > MaxWeightKg)
        {
            return null;
        }

        var package = _catalogue.GetPackages()
            .OrderBy(p => p.Cost)
            .FirstOrDefault(p => p.Fits(parcel));

        return package is null ? null : new PackagingResult(package.PackageType, package.Cost);
    }
}
