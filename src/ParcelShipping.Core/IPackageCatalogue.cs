namespace ParcelShipping.Core;

public interface IPackageCatalogue
{
    IReadOnlyList<PackageDefinition> GetPackages();
}
