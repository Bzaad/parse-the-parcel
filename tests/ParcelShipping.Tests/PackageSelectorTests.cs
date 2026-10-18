using ParcelShipping.Core;

namespace ParcelShipping.Tests;

public class PackageSelectorTests
{
    private readonly PackageSelector _selector = new(new InCodePackageCatalogue());

    public static TheoryData<decimal, decimal, decimal, PackageType, decimal> ParcelsThatFit => new()
    {
        // Exactly at each box's limits
        { 200m, 300m, 150m, PackageType.Small, 5.00m },
        { 300m, 400m, 200m, PackageType.Medium, 7.50m },
        { 400m, 600m, 250m, PackageType.Large, 8.50m },

        // 1mm over a box's limit moves up to the next box
        { 201m, 300m, 150m, PackageType.Medium, 7.50m },
        { 200m, 301m, 150m, PackageType.Medium, 7.50m },
        { 200m, 300m, 151m, PackageType.Medium, 7.50m },
        { 301m, 400m, 200m, PackageType.Large, 8.50m },
        { 300m, 401m, 200m, PackageType.Large, 8.50m },
        { 300m, 400m, 201m, PackageType.Large, 8.50m },

        // Rotated: the same shape with the sides in a different order
        { 400m, 300m, 200m, PackageType.Medium, 7.50m },
        { 150m, 200m, 300m, PackageType.Small, 5.00m },
        { 600m, 250m, 400m, PackageType.Large, 8.50m },

        // Smaller than the smallest box still gets the smallest box
        { 1m, 1m, 1m, PackageType.Small, 5.00m },
    };

    [Theory]
    [MemberData(nameof(ParcelsThatFit))]
    public void Select_ParcelFits_ReturnsCheapestPackage(
        decimal length, decimal breadth, decimal height, PackageType expectedType, decimal expectedCost)
    {
        var result = _selector.Select(new Parcel(length, breadth, height, weight: 1m));

        Assert.Equal(new PackagingResult(expectedType, expectedCost), result);
    }

    public static TheoryData<decimal, decimal, decimal> OversizedParcels => new()
    {
        // 1mm over Large's limits on each side
        { 401m, 600m, 250m },
        { 400m, 601m, 250m },
        { 400m, 600m, 251m },

        // Much too big
        { 1000m, 1000m, 1000m },

        // Less volume than Large, but no rotation gets every side inside it
        { 300m, 300m, 300m },
    };

    [Theory]
    [MemberData(nameof(OversizedParcels))]
    public void Select_ParcelTooBigForEveryBox_ReturnsNull(decimal length, decimal breadth, decimal height)
    {
        var result = _selector.Select(new Parcel(length, breadth, height, weight: 1m));

        Assert.Null(result);
    }

    [Fact]
    public void Select_ExactlyMaximumWeight_IsAccepted()
    {
        var result = _selector.Select(new Parcel(200m, 300m, 150m, weight: 25m));

        Assert.Equal(new PackagingResult(PackageType.Small, 5.00m), result);
    }

    [Fact]
    public void Select_JustOverMaximumWeight_ReturnsNull()
    {
        var result = _selector.Select(new Parcel(200m, 300m, 150m, weight: 25.01m));

        Assert.Null(result);
    }

    [Fact]
    public void Select_CatalogueNotListedByCost_StillReturnsCheapestFit()
    {
        var catalogue = new StubCatalogue(
            new PackageDefinition(PackageType.Large, 400m, 600m, 250m, 8.50m),
            new PackageDefinition(PackageType.Small, 200m, 300m, 150m, 5.00m),
            new PackageDefinition(PackageType.Medium, 300m, 400m, 200m, 7.50m));
        var selector = new PackageSelector(catalogue);

        var result = selector.Select(new Parcel(100m, 100m, 100m, weight: 1m));

        Assert.Equal(new PackagingResult(PackageType.Small, 5.00m), result);
    }

    [Fact]
    public void Select_WeightBelowLimit_DoesNotChangeTheBox()
    {
        var light = _selector.Select(new Parcel(300m, 400m, 200m, weight: 0.1m));
        var heavy = _selector.Select(new Parcel(300m, 400m, 200m, weight: 24.9m));

        Assert.Equal(light, heavy);
    }

    private sealed class StubCatalogue(params PackageDefinition[] packages) : IPackageCatalogue
    {
        public IReadOnlyList<PackageDefinition> GetPackages() => packages;
    }
}
