namespace OpenQuant.Portfolio;

/// <summary>
/// Represents a pending order to buy or sell a financial instrument.
/// </summary>
public sealed record Order
{
    /// <summary>Gets the unique identifier for the order.</summary>
    public required Guid Id { get; init; }

    /// <summary>Gets the instrument symbol this order applies to.</summary>
    public required string Symbol { get; init; }

    /// <summary>Gets the side of the order (buy or sell).</summary>
    public required OrderSide Side { get; init; }

    /// <summary>Gets the quantity to buy or sell.</summary>
    public required decimal Quantity { get; init; }

    /// <summary>Gets the price at which the order should be executed.</summary>
    public required decimal Price { get; init; }

    /// <summary>Gets the timestamp when the order was created.</summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>Gets or sets the status of the order.</summary>
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    /// <summary>Gets or sets the identifier of the lot created when this order is filled (sell side).</summary>
    public Guid? FilledLotId { get; set; }
}

/// <summary>
/// Represents the current state of an order.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// The order has been created but not yet executed.
    /// </summary>
    Pending,

    /// <summary>
    /// The order has been fully executed.
    /// </summary>
    Filled,

    /// <summary>
    /// The order was partially executed.
    /// </summary>
    PartiallyFilled,

    /// <summary>
    /// The order was cancelled before execution.
    /// </summary>
    Cancelled,
}
