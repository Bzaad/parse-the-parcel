using ParcelShipping.Core;

namespace ParcelShipping.Tests;

public class ParcelTests
{
    [Fact]
    public void Constructor_StoresValuesOnMatchingProperties()
    {
        var parcel = new Parcel(length: 100m, breadth: 200m, height: 300m, weight: 4.5m);

        Assert.Equal(100m, parcel.Length);
        Assert.Equal(200m, parcel.Breadth);
        Assert.Equal(300m, parcel.Height);
        Assert.Equal(4.5m, parcel.Weight);
    }

    [Theory]
    [InlineData("length", 0, 200, 300, 4.5)]
    [InlineData("length", -1, 200, 300, 4.5)]
    [InlineData("breadth", 100, 0, 300, 4.5)]
    [InlineData("breadth", 100, -1, 300, 4.5)]
    [InlineData("height", 100, 200, 0, 4.5)]
    [InlineData("height", 100, 200, -1, 4.5)]
    [InlineData("weight", 100, 200, 300, 0)]
    [InlineData("weight", 100, 200, 300, -1)]
    public void Constructor_ZeroOrNegativeValue_ThrowsNamingTheArgument(
        string expectedParamName, double length, double breadth, double height, double weight)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Parcel((decimal)length, (decimal)breadth, (decimal)height, (decimal)weight));

        Assert.Equal(expectedParamName, exception.ParamName);
    }

    [Fact]
    public void Constructor_TinyPositiveValues_AreAccepted()
    {
        var parcel = new Parcel(0.001m, 0.001m, 0.001m, 0.001m);

        Assert.Equal(0.001m, parcel.Length);
        Assert.Equal(0.001m, parcel.Breadth);
        Assert.Equal(0.001m, parcel.Height);
        Assert.Equal(0.001m, parcel.Weight);
    }
}
