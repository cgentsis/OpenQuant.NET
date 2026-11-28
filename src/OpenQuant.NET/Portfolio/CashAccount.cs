using System.Collections.Frozen;

namespace OpenQuant.Portfolio;

/// <summary>
/// Represents a cash account that tracks balances in multiple currencies.
/// </summary>
public sealed class CashAccount
{
    private readonly Dictionary<string, decimal> _balances = [];

    /// <summary>
    /// Gets the available balances keyed by currency code.
    /// </summary>
    public IReadOnlyDictionary<string, decimal> Balances => _balances.ToFrozenDictionary();

    /// <summary>
    /// Adds the specified amount to the balance in the given currency.
    /// </summary>
    /// <param name="currency">The currency code (e.g., "USD").</param>
    /// <param name="amount">The amount to add (can be negative to subtract).</param>
    public void Add(string currency, decimal amount)
    {
        _balances[currency] = _balances.GetValueOrDefault(currency, 0m) + amount;
    }

    /// <summary>
    /// Subtracts the specified amount from the balance in the given currency.
    /// </summary>
    /// <param name="currency">The currency code (e.g., "USD").</param>
    /// <param name="amount">The amount to subtract.</param>
    public void Subtract(string currency, decimal amount)
    {
        Add(currency, -amount);
    }

    /// <summary>
    /// Gets the current balance for a specific currency.
    /// </summary>
    /// <param name="currency">The currency code.</param>
    /// <returns>The balance, or zero if no balance exists for the currency.</returns>
    public decimal GetBalance(string currency) => _balances.GetValueOrDefault(currency, 0m);

    /// <summary>
    /// Checks whether the balance in the given currency is sufficient for the requested amount.
    /// </summary>
    /// <param name="currency">The currency code.</param>
    /// <param name="amount">The required amount.</param>
    /// <returns>True if the balance is greater than or equal to the amount.</returns>
    public bool HasSufficient(string currency, decimal amount) => GetBalance(currency) >= amount;
}
