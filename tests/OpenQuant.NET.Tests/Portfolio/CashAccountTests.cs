using OpenQuant.Portfolio;

namespace OpenQuant.Tests.Portfolio;

public class CashAccountTests
{
    [Fact]
    public void Add_InitialDeposit_SetsBalance()
    {
        var account = new CashAccount();
        account.Add("USD", 10000m);
        Assert.Equal(10000m, account.GetBalance("USD"));
    }

    [Fact]
    public void Add_MultipleDeposits_AccumulatesBalance()
    {
        var account = new CashAccount();
        account.Add("USD", 5000m);
        account.Add("USD", 3000m);
        Assert.Equal(8000m, account.GetBalance("USD"));
    }

    [Fact]
    public void Subtract_RemovesFromBalance()
    {
        var account = new CashAccount();
        account.Add("USD", 10000m);
        account.Subtract("USD", 2000m);
        Assert.Equal(8000m, account.GetBalance("USD"));
    }

    [Fact]
    public void Subtract_FromZero_BecomesNegative()
    {
        var account = new CashAccount();
        account.Subtract("USD", 500m);
        Assert.Equal(-500m, account.GetBalance("USD"));
    }

    [Fact]
    public void GetBalance_UnsupportedCurrency_ReturnsZero()
    {
        var account = new CashAccount();
        Assert.Equal(0m, account.GetBalance("EUR"));
    }

    [Fact]
    public void HasSufficient_EnoughBalance_ReturnsTrue()
    {
        var account = new CashAccount();
        account.Add("USD", 10000m);
        Assert.True(account.HasSufficient("USD", 5000m));
    }

    [Fact]
    public void HasSufficient_InsufficientBalance_ReturnsFalse()
    {
        var account = new CashAccount();
        account.Add("USD", 10000m);
        Assert.False(account.HasSufficient("USD", 15000m));
    }

    [Fact]
    public void HasSufficient_EqualBalance_ReturnsTrue()
    {
        var account = new CashAccount();
        account.Add("USD", 10000m);
        Assert.True(account.HasSufficient("USD", 10000m));
    }

    [Fact]
    public void Balances_ReturnsAllCurrencies()
    {
        var account = new CashAccount();
        account.Add("USD", 10000m);
        account.Add("EUR", 5000m);
        var balances = account.Balances;
        Assert.Equal(2, balances.Count);
        Assert.Equal(10000m, balances["USD"]);
        Assert.Equal(5000m, balances["EUR"]);
    }

    [Fact]
    public void Add_NegativeAmount_RemovesFromBalance()
    {
        var account = new CashAccount();
        account.Add("USD", 10000m);
        account.Add("USD", -3000m);
        Assert.Equal(7000m, account.GetBalance("USD"));
    }
}
