using OpenQuant.Portfolio;
using PortfolioType = global::OpenQuant.Portfolio.Portfolio;

namespace OpenQuant.Tests.Portfolio;

public class PortfolioTests
{
    [Fact]
    public void Constructor_InitializesWithBaseCurrency()
    {
        var portfolio = new PortfolioType("USD");
        Assert.Equal("USD", portfolio.BaseCurrency);
        Assert.Equal(0m, portfolio.GetEquity());
    }

    [Fact]
    public void SubmitOrder_AddsToPendingOrders()
    {
        var portfolio = new PortfolioType("USD");
        var order = new Order
        {
            Id = Guid.NewGuid(),
            Symbol = "AAPL",
            Side = OrderSide.Buy,
            Quantity = 100m,
            Price = 150m,
        };

        portfolio.SubmitOrder(order);
        Assert.Equal(OrderStatus.Pending, order.Status);
    }

    [Fact]
    public void Apply_BuyOrder_DeductsCashAndCreatesPosition()
    {
        var portfolio = new PortfolioType("USD");
        portfolio.CashAccount.Add("USD", 100000m);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            Symbol = "AAPL",
            Side = OrderSide.Buy,
            Quantity = 100m,
            Price = 150m,
        };

        portfolio.SubmitOrder(order);
        portfolio.Apply();

        Assert.Equal(OrderStatus.Filled, order.Status);
        Assert.NotNull(order.FilledLotId);
        Assert.Equal(85000m, portfolio.CashAccount.GetBalance("USD"));
        Assert.Equal(100m, portfolio.Positions["AAPL"].NetQuantity);
        Assert.Equal(150m, portfolio.Positions["AAPL"].AvgCost);
        Assert.Single(portfolio.OrderHistory);
        Assert.Single(portfolio.LotHistory);
    }

    [Fact]
    public void Apply_SellOrder_CreatesProceeds()
    {
        var portfolio = new PortfolioType("USD");
        portfolio.CashAccount.Add("USD", 100000m);

        // Buy first
        var buyOrder = new Order
        {
            Id = Guid.NewGuid(),
            Symbol = "AAPL",
            Side = OrderSide.Buy,
            Quantity = 100m,
            Price = 150m,
        };
        portfolio.SubmitOrder(buyOrder);
        portfolio.Apply();

        // Then sell
        var sellOrder = new Order
        {
            Id = Guid.NewGuid(),
            Symbol = "AAPL",
            Side = OrderSide.Sell,
            Quantity = 100m,
            Price = 170m,
        };
        portfolio.SubmitOrder(sellOrder);
        portfolio.Apply();

        Assert.Equal(OrderStatus.Filled, sellOrder.Status);
        Assert.Equal(102000m, portfolio.CashAccount.GetBalance("USD"));
        Assert.Equal(0m, portfolio.Positions["AAPL"].NetQuantity);
        Assert.Equal(2000m, portfolio.Positions["AAPL"].RealizedPnL);
    }

    [Fact]
    public void Apply_PartialSell_ClosesLotsFIFO()
    {
        var portfolio = new PortfolioType("USD");
        portfolio.CashAccount.Add("USD", 100000m);

        // Buy 100 shares at $150
        var buy1 = new Order
        {
            Id = Guid.NewGuid(),
            Symbol = "AAPL",
            Side = OrderSide.Buy,
            Quantity = 100m,
            Price = 150m,
        };
        portfolio.SubmitOrder(buy1);

        // Buy 100 more at $170
        var buy2 = new Order
        {
            Id = Guid.NewGuid(),
            Symbol = "AAPL",
            Side = OrderSide.Buy,
            Quantity = 100m,
            Price = 170m,
        };
        portfolio.SubmitOrder(buy2);
        portfolio.Apply();

        // Sell 100 shares at $180 - should close the first lot (FIFO)
        var sell = new Order
        {
            Id = Guid.NewGuid(),
            Symbol = "AAPL",
            Side = OrderSide.Sell,
            Quantity = 100m,
            Price = 180m,
        };
        portfolio.SubmitOrder(sell);
        portfolio.Apply();

        var position = portfolio.Positions["AAPL"];
        Assert.Equal(100m, position.NetQuantity);
        Assert.Equal(170m, position.AvgCost);
        Assert.Equal(3000m, position.RealizedPnL);
        Assert.Single(position.Lots);
        Assert.Equal(buy2.FilledLotId, position.Lots[0].Id);
    }

    [Fact]
    public void Apply_SellExceedingPosition_CreatesShortPosition()
    {
        var portfolio = new PortfolioType("USD");
        portfolio.CashAccount.Add("USD", 100000m);

        // Buy 100 shares at $150
        var buy = new Order
        {
            Id = Guid.NewGuid(),
            Symbol = "AAPL",
            Side = OrderSide.Buy,
            Quantity = 100m,
            Price = 150m,
        };
        portfolio.SubmitOrder(buy);
        portfolio.Apply();

        // Sell 150 shares at $160
        var sell = new Order
        {
            Id = Guid.NewGuid(),
            Symbol = "AAPL",
            Side = OrderSide.Sell,
            Quantity = 150m,
            Price = 160m,
        };
        portfolio.SubmitOrder(sell);
        portfolio.Apply();

        var position = portfolio.Positions["AAPL"];
        Assert.Equal(-50m, position.NetQuantity);
        Assert.Equal(160m, position.AvgCost);
        Assert.Equal(1000m, position.RealizedPnL);
    }

    [Fact]
    public void ConvertCurrency_SameCurrency_ReturnsSameAmount()
    {
        var portfolio = new PortfolioType("USD");
        Assert.Equal(1000m, portfolio.ConvertCurrency(1000m, "USD", "USD"));
    }

    [Fact]
    public void ConvertCurrency_FromBaseToForeign_MultipliesByRate()
    {
        var fxRates = new Dictionary<string, decimal> { ["EUR"] = 0.85m };
        var portfolio = new PortfolioType("USD", fxRates);
        var result = portfolio.ConvertFromBase(1000m, "EUR");
        Assert.Equal(850m, result);
    }

    [Fact]
    public void ConvertCurrency_FromForeignToBase_DividesByRate()
    {
        var fxRates = new Dictionary<string, decimal> { ["EUR"] = 0.85m };
        var portfolio = new PortfolioType("USD", fxRates);
        var result = portfolio.ConvertToBase(850m, "EUR");
        Assert.Equal(1000m, result);
    }

    [Fact]
    public void ConvertCurrency_CrossCurrency_ViaBase()
    {
        var fxRates = new Dictionary<string, decimal>
        {
            ["EUR"] = 0.85m,
            ["GBP"] = 0.75m,
        };
        var portfolio = new PortfolioType("USD", fxRates);

        // Convert 1000 EUR to GBP via USD
        var inUSD = portfolio.ConvertToBase(1000m, "EUR");
        var inGBP = portfolio.ConvertFromBase(inUSD, "GBP");
        Assert.Equal(882.3529411764705882352941176m, inGBP);
    }

    [Fact]
    public void GetEquity_OnlyCash_ReturnsCashBalance()
    {
        var portfolio = new PortfolioType("USD");
        portfolio.CashAccount.Add("USD", 50000m);
        Assert.Equal(50000m, portfolio.GetEquity());
    }

    [Fact]
    public void GetEquity_WithPosition_AddsMarketValue()
    {
        var portfolio = new PortfolioType("USD");
        portfolio.CashAccount.Add("USD", 100000m);

        var buy = new Order
        {
            Id = Guid.NewGuid(),
            Symbol = "AAPL",
            Side = OrderSide.Buy,
            Quantity = 100m,
            Price = 150m,
        };
        portfolio.SubmitOrder(buy);
        portfolio.Apply();

        var equity = portfolio.GetEquity(new Dictionary<string, decimal> { ["AAPL"] = 170m });

        Assert.Equal(102000m, equity);
    }

    [Fact]
    public void GetEquity_WithForeignCash_ConvertsToBase()
    {
        var fxRates = new Dictionary<string, decimal> { ["EUR"] = 0.85m };
        var portfolio = new PortfolioType("USD", fxRates);
        portfolio.CashAccount.Add("USD", 80000m);
        portfolio.CashAccount.Add("EUR", 10000m);

        var equity = portfolio.GetEquity();
        Assert.Equal(91764.70588235294117647058824m, equity);
    }

    [Fact]
    public void Snapshot_ReturnsCompleteState()
    {
        var portfolio = new PortfolioType("USD");
        portfolio.CashAccount.Add("USD", 100000m);

        var buy = new Order
        {
            Id = Guid.NewGuid(),
            Symbol = "AAPL",
            Side = OrderSide.Buy,
            Quantity = 100m,
            Price = 150m,
        };
        portfolio.SubmitOrder(buy);
        portfolio.Apply();

        var snapshot = portfolio.Snapshot(new Dictionary<string, decimal> { ["AAPL"] = 170m });

        Assert.Equal(102000m, snapshot.TotalEquity);
        Assert.Equal(85000m, snapshot.CashBalances["USD"]);
        Assert.Equal(17000m, snapshot.PositionValues["AAPL"]);
        Assert.NotEqual(DateTimeOffset.MinValue, snapshot.Timestamp);
    }

    [Fact]
    public void Snapshot_EmptyPortfolio_ReturnsZeroEquity()
    {
        var portfolio = new PortfolioType("USD");
        var snapshot = portfolio.Snapshot();
        Assert.Equal(0m, snapshot.TotalEquity);
        Assert.Empty(snapshot.PositionValues);
    }

    [Fact]
    public void OrderHistory_TracksAllOrders()
    {
        var portfolio = new PortfolioType("USD");
        portfolio.CashAccount.Add("USD", 100000m);

        var order1 = new Order
        {
            Id = Guid.NewGuid(),
            Symbol = "AAPL",
            Side = OrderSide.Buy,
            Quantity = 100m,
            Price = 150m,
        };
        var order2 = new Order
        {
            Id = Guid.NewGuid(),
            Symbol = "AAPL",
            Side = OrderSide.Sell,
            Quantity = 50m,
            Price = 160m,
        };

        portfolio.SubmitOrder(order1);
        portfolio.SubmitOrder(order2);
        portfolio.Apply();

        Assert.Equal(2, portfolio.OrderHistory.Count);
        Assert.Same(order1, portfolio.OrderHistory[0]);
        Assert.Same(order2, portfolio.OrderHistory[1]);
    }

    [Fact]
    public void LotHistory_TracksAllLots()
    {
        var portfolio = new PortfolioType("USD");
        portfolio.CashAccount.Add("USD", 100000m);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            Symbol = "AAPL",
            Side = OrderSide.Buy,
            Quantity = 100m,
            Price = 150m,
        };

        portfolio.SubmitOrder(order);
        portfolio.Apply();

        Assert.Single(portfolio.LotHistory);
        Assert.Equal("AAPL", portfolio.LotHistory[0].Symbol);
        Assert.Equal(OrderSide.Buy, portfolio.LotHistory[0].Side);
        Assert.Equal(100m, portfolio.LotHistory[0].OriginalQuantity);
        Assert.Equal(100m, portfolio.LotHistory[0].RemainingQuantity);
        Assert.Equal(150m, portfolio.LotHistory[0].Price);
    }
}
