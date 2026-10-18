namespace ParcelShipping.Core;

/// <summary>A parcel to be shipped. Dimensions are in millimetres, weight is in kilograms.</summary>
public sealed record Parcel
{
    public Parcel(decimal length, decimal breadth, decimal height, decimal weight)
    {
        Length = RequirePositive(length, nameof(length));
        Breadth = RequirePositive(breadth, nameof(breadth));
        Height = RequirePositive(height, nameof(height));
        Weight = RequirePositive(weight, nameof(weight));
    }

    public decimal Length { get; }
    public decimal Breadth { get; }
    public decimal Height { get; }
    public decimal Weight { get; }

    private static decimal RequirePositive(decimal value, string name) =>
        value > 0
            ? value
            : throw new ArgumentOutOfRangeException(name, value, "Value must be greater than zero.");
}
