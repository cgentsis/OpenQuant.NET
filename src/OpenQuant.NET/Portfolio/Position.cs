using System.Collections.Frozen;

namespace OpenQuant.Portfolio;

/// <summary>
/// Represents an open position in a financial instrument, tracking lots, quantities, and PnL.
/// </summary>
public sealed class Position
{
    private readonly List<Lot> _lots = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="Position"/> class for the specified instrument symbol.
    /// </summary>
    /// <param name="symbol">The instrument symbol.</param>
    public Position(string symbol)
    {
        Symbol = symbol;
    }

    /// <summary>Gets the instrument symbol for this position.</summary>
    public string Symbol { get; }

    /// <summary>Gets the net quantity of the position (positive for long, negative for short).</summary>
    public decimal NetQuantity { get; private set; }

    /// <summary>Gets the weighted average cost of the position.</summary>
    public decimal AvgCost { get; private set; }

    /// <summary>Gets the realized profit and loss from closed lots.</summary>
    public decimal RealizedPnL { get; private set; }

    /// <summary>Gets the list of open lots in FIFO order.</summary>
    public IReadOnlyList<Lot> Lots => _lots.AsReadOnly();

    /// <summary>
    /// Gets a frozen dictionary of lots keyed by their GUID for O(1) lookup.
    /// </summary>
    public FrozenDictionary<Guid, Lot> LotsById { get; private set; } = FrozenDictionary<Guid, Lot>.Empty;

    /// <summary>
    /// Adds a lot to the position.
    /// </summary>
    /// <param name="lot">The lot to add.</param>
    public void AddLot(Lot lot)
    {
        if (lot.Side == OrderSide.Buy)
        {
            if (NetQuantity >= 0m)
            {
                // Adding to a long position or closing a short
                var totalCost = (NetQuantity * AvgCost) + (lot.RemainingQuantity * lot.Price);
                NetQuantity += lot.RemainingQuantity;
                AvgCost = NetQuantity > 0m ? totalCost / NetQuantity : 0m;
            }
            else
            {
                // We are short; a buy reduces the short
                RealizedPnL += (AvgCost - lot.Price) * Math.Min(lot.RemainingQuantity, -NetQuantity);
                NetQuantity += lot.RemainingQuantity;
                if (NetQuantity > 0m)
                {
                    // Flipped from short to long; reset avg cost
                    AvgCost = lot.Price;
                }
                else if (NetQuantity == 0m)
                {
                    AvgCost = 0m;
                }
            }
        }
        else
        {
            if (NetQuantity <= 0m)
            {
                // Adding to a short position or closing a long
                var totalCost = (-NetQuantity * AvgCost) + (lot.RemainingQuantity * lot.Price);
                NetQuantity -= lot.RemainingQuantity;
                AvgCost = NetQuantity < 0m ? totalCost / (-NetQuantity) : 0m;
            }
            else
            {
                // We are long; a sell reduces the long
                RealizedPnL += (lot.Price - AvgCost) * Math.Min(lot.RemainingQuantity, NetQuantity);
                NetQuantity -= lot.RemainingQuantity;
                if (NetQuantity < 0m)
                {
                    // Flipped from long to short
                    AvgCost = lot.Price;
                }
                else if (NetQuantity == 0m)
                {
                    AvgCost = 0m;
                }
            }
        }

        _lots.Add(lot);
        LotsById = _lots.ToFrozenDictionary(l => l.Id);
    }

    /// <summary>
    /// Closes lots using FIFO order, returning the realized PnL from the closed quantity.
    /// </summary>
    /// <param name="quantity">The quantity to close (positive closes long, negative closes short).</param>
    /// <returns>The realized PnL from closing the specified quantity.</returns>
    public decimal Close(decimal quantity)
    {
        return Close(quantity, marketPrice: 0m);
    }

    /// <summary>
    /// Closes lots using FIFO order at the specified price, returning the realized PnL.
    /// </summary>
    /// <param name="quantity">The quantity to close (positive closes long, negative closes short).</param>
    /// <param name="marketPrice">The execution price. If 0, uses the lot price for PnL calculation.</param>
    /// <returns>The realized PnL from closing the specified quantity.</returns>
    public decimal Close(decimal quantity, decimal marketPrice)
    {
        if (quantity == 0m)
        {
            return 0m;
        }

        var pnl = 0m;
        var remaining = quantity;

        while (remaining != 0m && _lots.Count > 0)
        {
            var lot = _lots[0];
            if (lot.Side == OrderSide.Buy)
            {
                var closeQty = Math.Min(remaining, lot.RemainingQuantity);
                if (closeQty > 0m)
                {
                    pnl += (marketPrice > 0m ? marketPrice - lot.Price : lot.Price - AvgCost) * closeQty;
                    var closed = lot.Close(closeQty);
                    remaining -= closed;
                    NetQuantity -= closed;
                    if (lot.RemainingQuantity == 0m)
                    {
                        _lots.RemoveAt(0);
                        LotsById = _lots.ToFrozenDictionary(l => l.Id);
                    }
                }
            }
            else
            {
                var closeQty = Math.Min(remaining, lot.RemainingQuantity);
                if (closeQty > 0m)
                {
                    pnl += (marketPrice > 0m ? lot.Price - marketPrice : AvgCost - lot.Price) * closeQty;
                    var closed = lot.Close(closeQty);
                    remaining += closed;
                    NetQuantity += closed;
                    if (lot.RemainingQuantity == 0m)
                    {
                        _lots.RemoveAt(0);
                        LotsById = _lots.ToFrozenDictionary(l => l.Id);
                    }
                }
            }
        }

        RealizedPnL += pnl;

        // Update AvgCost to reflect remaining lots after closing
        if (NetQuantity == 0m)
        {
            AvgCost = 0m;
        }
        else if (NetQuantity > 0m && _lots.Count > 0)
        {
            var totalCost = _lots.Sum(l => l.RemainingQuantity * l.Price);
            AvgCost = totalCost / NetQuantity;
        }
        else if (NetQuantity < 0m && _lots.Count > 0)
        {
            var totalCost = _lots.Sum(l => l.RemainingQuantity * l.Price);
            AvgCost = totalCost / (-NetQuantity);
        }

        return pnl;
    }

    /// <summary>
    /// Computes the unrealized profit and loss at the given market price.
    /// </summary>
    /// <param name="marketPrice">The current market price of the instrument.</param>
    /// <returns>The unrealized PnL.</returns>
    public decimal ComputeUnrealizedPnL(decimal marketPrice)
    {
        if (NetQuantity == 0m)
        {
            return 0m;
        }

        return NetQuantity * (marketPrice - AvgCost);
    }
}
