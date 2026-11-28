using System.Collections.Frozen;

namespace OpenQuant.Portfolio;

/// <summary>
/// Manages a portfolio of positions, cash accounts, and order execution.
/// </summary>
public sealed class Portfolio
{
    private readonly Dictionary<string, Position> _positions = [];
    private readonly CashAccount _cashAccount;
    private readonly Dictionary<string, decimal> _fxRates;
    private readonly List<Order> _orderHistory = [];
    private readonly List<Lot> _lotHistory = [];
    private readonly List<Order> _pendingOrders = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="Portfolio"/> class with the specified base currency.
    /// </summary>
    /// <param name="baseCurrency">The base currency code (e.g., "USD").</param>
    /// <param name="fxRates">Optional foreign exchange rates to the base currency.</param>
    public Portfolio(string baseCurrency, Dictionary<string, decimal>? fxRates = null)
    {
        BaseCurrency = baseCurrency;
        _cashAccount = new CashAccount();
        _fxRates = fxRates ?? new Dictionary<string, decimal>();
    }

    /// <summary>Gets the cash account for this portfolio.</summary>
    public CashAccount CashAccount => _cashAccount;

    /// <summary>Gets the available positions keyed by instrument symbol.</summary>
    public IReadOnlyDictionary<string, Position> Positions => _positions.ToFrozenDictionary();

    /// <summary>Gets the order history for this portfolio.</summary>
    public IReadOnlyList<Order> OrderHistory => _orderHistory.AsReadOnly();

    /// <summary>Gets the lot history for this portfolio.</summary>
    public IReadOnlyList<Lot> LotHistory => _lotHistory.AsReadOnly();

    /// <summary>Gets the base currency for equity calculations.</summary>
    public string BaseCurrency { get; }

    /// <summary>
    /// Submits an order for execution and returns the resulting order.
    /// </summary>
    /// <param name="order">The order to execute.</param>
    /// <returns>The executed order with updated status.</returns>
    public Order SubmitOrder(Order order)
    {
        _pendingOrders.Add(order);
        return order;
    }

    /// <summary>
    /// Applies all pending orders, executing them against the portfolio.
    /// </summary>
    public void Apply()
    {
        foreach (var order in _pendingOrders)
        {
            ExecuteOrder(order);
        }

        _pendingOrders.Clear();
    }

    /// <summary>
    /// Converts a cash amount from one currency to the base currency using the current FX rates.
    /// </summary>
    /// <param name="amount">The amount to convert.</param>
    /// <param name="fromCurrency">The source currency code.</param>
    /// <param name="toCurrency">The target currency code (defaults to base currency).</param>
    /// <returns>The converted amount.</returns>
    public decimal ConvertCurrency(decimal amount, string fromCurrency, string? toCurrency = null)
    {
        toCurrency ??= BaseCurrency;

        if (fromCurrency == toCurrency)
        {
            return amount;
        }

        // Convert via base currency as the intermediary
        var amountInBase = ConvertToBase(amount, fromCurrency);
        return ConvertFromBase(amountInBase, toCurrency);
    }

    /// <summary>
    /// Converts an amount from the base currency to a target currency.
    /// </summary>
    /// <param name="amount">The amount in base currency.</param>
    /// <param name="toCurrency">The target currency code.</param>
    /// <returns>The converted amount.</returns>
    public decimal ConvertFromBase(decimal amount, string toCurrency)
    {
        if (toCurrency == BaseCurrency)
        {
            return amount;
        }

        var rate = _fxRates.GetValueOrDefault(toCurrency, 1m);
        return rate > 0m ? amount * rate : amount;
    }

    /// <summary>
    /// Converts an amount from a source currency to the base currency.
    /// </summary>
    /// <param name="amount">The amount in the source currency.</param>
    /// <param name="fromCurrency">The source currency code.</param>
    /// <returns>The converted amount in base currency.</returns>
    public decimal ConvertToBase(decimal amount, string fromCurrency)
    {
        if (fromCurrency == BaseCurrency)
        {
            return amount;
        }

        var rate = _fxRates.GetValueOrDefault(fromCurrency, 1m);
        return rate > 0m ? amount / rate : amount;
    }

    /// <summary>
    /// Gets the total portfolio equity converted to the base currency.
    /// </summary>
    /// <param name="marketPrices">Optional market prices for positions, keyed by symbol.</param>
    /// <returns>The total equity in the base currency.</returns>
    public decimal GetEquity(Dictionary<string, decimal>? marketPrices = null)
    {
        var equity = _cashAccount.GetBalance(BaseCurrency);

        foreach (var kvp in _cashAccount.Balances)
        {
            if (kvp.Key != BaseCurrency)
            {
                equity += ConvertToBase(kvp.Value, kvp.Key);
            }
        }

        marketPrices ??= new Dictionary<string, decimal>();

        foreach (var position in _positions.Values)
        {
            var marketPrice = marketPrices.GetValueOrDefault(position.Symbol);
            if (marketPrice > 0m)
            {
                equity += position.NetQuantity * marketPrice;
            }
        }

        return equity;
    }

    /// <summary>
    /// Creates a snapshot of the current portfolio state.
    /// </summary>
    /// <param name="marketPrices">Optional market prices for positions, keyed by symbol.</param>
    /// <returns>A portfolio snapshot.</returns>
    public PortfolioSnapshot Snapshot(Dictionary<string, decimal>? marketPrices = null)
    {
        marketPrices ??= new Dictionary<string, decimal>();

        var cashBalances = new Dictionary<string, decimal>(_cashAccount.Balances);
        var positionValues = new Dictionary<string, decimal>();

        foreach (var position in _positions.Values)
        {
            var marketPrice = marketPrices.GetValueOrDefault(position.Symbol);
            if (marketPrice > 0m)
            {
                positionValues[position.Symbol] = position.NetQuantity * marketPrice;
            }
            else
            {
                positionValues[position.Symbol] = 0m;
            }
        }

        return new PortfolioSnapshot
        {
            TotalEquity = GetEquity(marketPrices),
            CashBalances = cashBalances,
            PositionValues = positionValues,
            Timestamp = DateTimeOffset.UtcNow,
        };
    }

    /// <summary>
    /// Gets or creates a position for the specified symbol.
    /// </summary>
    /// <param name="symbol">The instrument symbol.</param>
    /// <returns>The position for the symbol.</returns>
    private Position GetOrCreatePosition(string symbol)
    {
        if (!_positions.TryGetValue(symbol, out var position))
        {
            position = new Position(symbol);
            _positions[symbol] = position;
        }

        return position;
    }

    /// <summary>
    /// Executes a single order against the portfolio.
    /// </summary>
    /// <param name="order">The order to execute.</param>
    private void ExecuteOrder(Order order)
    {
        var position = GetOrCreatePosition(order.Symbol);
        var lot = CreateLot(order);

        if (order.Side == OrderSide.Sell && position.NetQuantity > 0m)
        {
            // Closing a long position using FIFO
            var closeQty = Math.Min(order.Quantity, position.NetQuantity);
            if (closeQty > 0m)
            {
                position.Close(closeQty, marketPrice: order.Price);
            }

            var excess = order.Quantity - closeQty;
            if (excess > 0m)
            {
                var excessLot = new Lot
                {
                    Id = Guid.NewGuid(),
                    Symbol = order.Symbol,
                    Side = order.Side,
                    OriginalQuantity = excess,
                    RemainingQuantity = excess,
                    Price = order.Price,
                    CreatedAt = order.CreatedAt,
                };
                _lotHistory.Add(excessLot);
                position.AddLot(excessLot);
            }

            // Credit proceeds for the entire order quantity
            var proceeds = order.Quantity * order.Price;
            _cashAccount.Add(BaseCurrency, proceeds);
        }
        else if (order.Side == OrderSide.Buy && position.NetQuantity < 0m)
        {
            // Covering a short position using FIFO
            var closeQty = Math.Min(order.Quantity, -position.NetQuantity);
            if (closeQty > 0m)
            {
                position.Close(closeQty, marketPrice: order.Price);
            }

            var excess = order.Quantity - closeQty;
            if (excess > 0m)
            {
                var excessLot = new Lot
                {
                    Id = Guid.NewGuid(),
                    Symbol = order.Symbol,
                    Side = order.Side,
                    OriginalQuantity = excess,
                    RemainingQuantity = excess,
                    Price = order.Price,
                    CreatedAt = order.CreatedAt,
                };
                _lotHistory.Add(excessLot);
                position.AddLot(excessLot);
            }

            // Debit cost for the entire order quantity
            var cost = order.Quantity * order.Price;
            _cashAccount.Subtract(BaseCurrency, cost);
        }
        else
        {
            // No existing position to close — just add the lot normally
            _lotHistory.Add(lot);
            position.AddLot(lot);

            if (order.Side == OrderSide.Buy)
            {
                _cashAccount.Subtract(BaseCurrency, order.Quantity * order.Price);
            }
            else
            {
                _cashAccount.Add(BaseCurrency, order.Quantity * order.Price);
            }
        }

        order.Status = OrderStatus.Filled;
        order.FilledLotId = lot.Id;
        _orderHistory.Add(order);
    }

    /// <summary>
    /// Creates a lot from the given order.
    /// </summary>
    /// <param name="order">The order to create a lot for.</param>
    /// <returns>The created lot.</returns>
    private Lot CreateLot(Order order)
    {
        return new Lot
        {
            Id = Guid.NewGuid(),
            Symbol = order.Symbol,
            Side = order.Side,
            OriginalQuantity = order.Quantity,
            RemainingQuantity = order.Quantity,
            Price = order.Price,
            CreatedAt = order.CreatedAt,
        };
    }
}
