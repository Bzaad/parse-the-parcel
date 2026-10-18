namespace ParcelShipping.Core;

/// <summary>A package type with the largest parcel it accepts (mm) and its cost.</summary>
public sealed record PackageDefinition(
    PackageType PackageType,
    decimal Length,
    decimal Breadth,
    decimal Height,
    decimal Cost)
{
    /// <summary>
    /// A parcel fits when, with both sets of sides sorted smallest to largest, each parcel side
    /// is no larger than the matching package side. This allows rotation and exact fits.
    /// </summary>
    public bool Fits(Parcel parcel)
    {
        var parcelSides = Sorted(parcel.Length, parcel.Breadth, parcel.Height);
        var packageSides = Sorted(Length, Breadth, Height);

        return parcelSides[0] <= packageSides[0]
            && parcelSides[1] <= packageSides[1]
            && parcelSides[2] <= packageSides[2];
    }

    private static decimal[] Sorted(decimal a, decimal b, decimal c)
    {
        var sides = new[] { a, b, c };
        Array.Sort(sides);
        return sides;
    }
}
