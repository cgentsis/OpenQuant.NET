using OpenQuant.Portfolio;

namespace OpenQuant.Tests.Portfolio;

public class PositionTests
{
    [Fact]
    public void Constructor_InitializesWithZeroQuantity()
    {
        var position = new Position("AAPL");
        Assert.Equal("AAPL", position.Symbol);
        Assert.Equal(0m, position.NetQuantity);
        Assert.Equal(0m, position.AvgCost);
        Assert.Equal(0m, position.RealizedPnL);
        Assert.Empty(position.Lots);
    }

    [Fact]
    public void AddLot_Buy_AddsToPosition()
    {
        var position = new Position("AAPL");
        var lot = new Lot
        {
            Id = Guid.NewGuid(),
            Symbol = "AAPL",
            Side = OrderSide.Buy,
            OriginalQuantity = 100m,
            RemainingQuantity = 100m,
            Price = 150m,
        };

        position.AddLot(lot);

        Assert.Equal(100m, position.NetQuantity);
        Assert.Equal(150m, position.AvgCost);
        Assert.Single(position.Lots);
    }

    [Fact]
    public void AddLot_SecondBuy_AveragesCost()
    {
        var position = new Position("AAPL");

        var lot1 = new Lot
        {
            Id = Guid.NewGuid(),
            Symbol = "AAPL",
            Side = OrderSide.Buy,
            OriginalQuantity = 100m,
            RemainingQuantity = 100m,
            Price = 150m,
        };

        var lot2 = new Lot
        {
            Id = Guid.NewGuid(),
            Symbol = "AAPL",
            Side = OrderSide.Buy,
            OriginalQuantity = 100m,
            RemainingQuantity = 100m,
            Price = 170m,
        };

        position.AddLot(lot1);
        position.AddLot(lot2);

        Assert.Equal(200m, position.NetQuantity);
        Assert.Equal(160m, position.AvgCost);
        Assert.Equal(2, position.Lots.Count);
    }

    [Fact]
    public void AddLot_Sell_ClosesLongPosition()
    {
        var position = new Position("AAPL");

        var buyLot = new Lot
        {
            Id = Guid.NewGuid(),
            Symbol = "AAPL",
            Side = OrderSide.Buy,
            OriginalQuantity = 100m,
            RemainingQuantity = 100m,
            Price = 150m,
        };

        var sellLot = new Lot
        {
            Id = Guid.NewGuid(),
            Symbol = "AAPL",
            Side = OrderSide.Sell,
            OriginalQuantity = 100m,
            RemainingQuantity = 100m,
            Price = 170m,
        };

        position.AddLot(buyLot);
        position.AddLot(sellLot);

        Assert.Equal(0m, position.NetQuantity);
        Assert.Equal(0m, position.AvgCost);
        Assert.Equal(2000m, position.RealizedPnL);
    }

    [Fact]
    public void AddLot_ShortSell_CreatesShortPosition()
    {
        var position = new Position("TSLA");

        var sellLot = new Lot
        {
            Id = Guid.NewGuid(),
            Symbol = "TSLA",
            Side = OrderSide.Sell,
            OriginalQuantity = 50m,
            RemainingQuantity = 50m,
            Price = 200m,
        };

        position.AddLot(sellLot);

        Assert.Equal(-50m, position.NetQuantity);
        Assert.Equal(200m, position.AvgCost);
    }

    [Fact]
    public void AddLot_CoverShort_ReducesShortPosition()
    {
        var position = new Position("TSLA");

        var sellLot = new Lot
        {
            Id = Guid.NewGuid(),
            Symbol = "TSLA",
            Side = OrderSide.Sell,
            OriginalQuantity = 100m,
            RemainingQuantity = 100m,
            Price = 200m,
        };

        var buyLot = new Lot
        {
            Id = Guid.NewGuid(),
            Symbol = "TSLA",
            Side = OrderSide.Buy,
            OriginalQuantity = 50m,
            RemainingQuantity = 50m,
            Price = 180m,
        };

        position.AddLot(sellLot);
        position.AddLot(buyLot);

        Assert.Equal(-50m, position.NetQuantity);
        Assert.Equal(200m, position.AvgCost);
        Assert.Equal(1000m, position.RealizedPnL);
    }

    [Fact]
    public void Close_FullClose_ReturnsRealizedPnL()
    {
        var position = new Position("AAPL");

        var lot = new Lot
        {
            Id = Guid.NewGuid(),
            Symbol = "AAPL",
            Side = OrderSide.Buy,
            OriginalQuantity = 100m,
            RemainingQuantity = 100m,
            Price = 150m,
        };

        position.AddLot(lot);
        var pnl = position.Close(100m);

        Assert.Equal(0m, position.NetQuantity);
        Assert.Equal(0m, position.AvgCost);
        Assert.Equal(0m, pnl);
        Assert.Equal(0m, position.RealizedPnL);
    }

    [Fact]
    public void Close_PartialClose_ClosesLotsFIFO()
    {
        var position = new Position("AAPL");

        var lot1 = new Lot
        {
            Id = Guid.NewGuid(),
            Symbol = "AAPL",
            Side = OrderSide.Buy,
            OriginalQuantity = 100m,
            RemainingQuantity = 100m,
            Price = 150m,
        };

        var lot2 = new Lot
        {
            Id = Guid.NewGuid(),
            Symbol = "AAPL",
            Side = OrderSide.Buy,
            OriginalQuantity = 100m,
            RemainingQuantity = 100m,
            Price = 170m,
        };

        position.AddLot(lot1);
        position.AddLot(lot2);

        // Close 100 shares FIFO: lot1 (price 150) is closed against avg cost 160
        var pnl = position.Close(100m);

        Assert.Equal(100m, position.NetQuantity);
        Assert.Equal(170m, position.AvgCost);
        Assert.Equal(-1000m, pnl);
        Assert.Equal(-1000m, position.RealizedPnL);
        Assert.Single(position.Lots);
    }

    [Fact]
    public void Close_ExceedsQuantity_ClosesAll()
    {
        var position = new Position("AAPL");

        var lot = new Lot
        {
            Id = Guid.NewGuid(),
            Symbol = "AAPL",
            Side = OrderSide.Buy,
            OriginalQuantity = 100m,
            RemainingQuantity = 100m,
            Price = 150m,
        };

        position.AddLot(lot);
        var pnl = position.Close(150m);

        Assert.Equal(0m, position.NetQuantity);
        Assert.Equal(0m, pnl);
    }

    [Fact]
    public void ComputeUnrealizedPnL_LongPosition_Correct()
    {
        var position = new Position("AAPL");

        var lot = new Lot
        {
            Id = Guid.NewGuid(),
            Symbol = "AAPL",
            Side = OrderSide.Buy,
            OriginalQuantity = 100m,
            RemainingQuantity = 100m,
            Price = 150m,
        };

        position.AddLot(lot);
        var unrealized = position.ComputeUnrealizedPnL(170m);

        Assert.Equal(2000m, unrealized);
    }

    [Fact]
    public void ComputeUnrealizedPnL_ShortPosition_Correct()
    {
        var position = new Position("TSLA");

        var lot = new Lot
        {
            Id = Guid.NewGuid(),
            Symbol = "TSLA",
            Side = OrderSide.Sell,
            OriginalQuantity = 50m,
            RemainingQuantity = 50m,
            Price = 200m,
        };

        position.AddLot(lot);
        var unrealized = position.ComputeUnrealizedPnL(180m);

        Assert.Equal(1000m, unrealized);
    }

    [Fact]
    public void ComputeUnrealizedPnL_EmptyPosition_ReturnsZero()
    {
        var position = new Position("AAPL");
        Assert.Equal(0m, position.ComputeUnrealizedPnL(150m));
    }

    [Fact]
    public void LotsById_ReturnsLookupByGuid()
    {
        var position = new Position("AAPL");
        var lotId = Guid.NewGuid();
        var lot = new Lot
        {
            Id = lotId,
            Symbol = "AAPL",
            Side = OrderSide.Buy,
            OriginalQuantity = 100m,
            RemainingQuantity = 100m,
            Price = 150m,
        };

        position.AddLot(lot);

        Assert.True(position.LotsById.ContainsKey(lotId));
        Assert.Same(lot, position.LotsById[lotId]);
    }

    [Fact]
    public void Close_ZeroQuantity_ReturnsZero()
    {
        var position = new Position("AAPL");
        var pnl = position.Close(0m);
        Assert.Equal(0m, pnl);
    }
}
