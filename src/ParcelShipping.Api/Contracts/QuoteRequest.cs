namespace ParcelShipping.Api.Contracts;

/// <summary>A parcel to quote. Dimensions are in millimetres, weight is in kilograms.</summary>
public sealed record QuoteRequest(decimal Length, decimal Breadth, decimal Height, decimal Weight);
