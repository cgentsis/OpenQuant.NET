namespace OpenQuant.Portfolio;

/// <summary>
/// Represents a lot (a batch purchase) that contributes to an open position.
/// </summary>
public sealed record Lot
{
    /// <summary>Gets the unique identifier for this lot.</summary>
    public required Guid Id { get; init; }

    /// <summary>Gets the instrument symbol this lot belongs to.</summary>
    public required string Symbol { get; init; }

    /// <summary>Gets the side of the original order (Buy or Sell).</summary>
    public required OrderSide Side { get; init; }

    /// <summary>Gets the original quantity of the lot.</summary>
    public required decimal OriginalQuantity { get; init; }

    /// <summary>Gets or sets the remaining quantity still open in this lot.</summary>
    public decimal RemainingQuantity { get; set; }

    /// <summary>Gets the price at which the lot was established.</summary>
    public required decimal Price { get; init; }

    /// <summary>Gets the timestamp when the lot was created.</summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Reduces the remaining quantity of this lot by the specified amount.
    /// </summary>
    /// <param name="quantity">The quantity to close from this lot.</param>
    /// <returns>The actual quantity closed, which may be less than requested if the lot is partially filled.</returns>
    public decimal Close(decimal quantity)
    {
        var actual = Math.Min(quantity, RemainingQuantity);
        RemainingQuantity -= actual;
        return actual;
    }
}
