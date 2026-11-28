using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;
using OpenQuant.Models;

namespace OpenQuant.Tests.Analysis;

/// <summary>
/// Tests for the arithmetic operator factories.
/// </summary>
public class MathOperatorsTests
{
    [Fact]
    public async Task Add_ComputesHighPlusLow()
    {
        var inputs = new[]
        {
            ExtraIndicatorTestData.Ohlcv(10m, 20m, 15m, 18m),
            ExtraIndicatorTestData.Ohlcv(30m, 50m, 40m, 45m),
        };

        var results = await ExtraIndicatorTestData.RunAsync(MathOperators.Add(10, "ADD"), inputs);

        Assert.Equal(35m, results[0].Indicators["ADD"]);
        Assert.Equal(90m, results[1].Indicators["ADD"]);
    }

    [Fact]
    public async Task Sub_ComputesHighMinusLow()
    {
        var inputs = new[]
        {
            ExtraIndicatorTestData.Ohlcv(10m, 20m, 10m, 15m),
            ExtraIndicatorTestData.Ohlcv(30m, 50m, 30m, 40m),
        };

        var results = await ExtraIndicatorTestData.RunAsync(MathOperators.Sub(10, "SUB"), inputs);

        Assert.Equal(10m, results[0].Indicators["SUB"]);
        Assert.Equal(20m, results[1].Indicators["SUB"]);
    }

    [Fact]
    public async Task Mult_ComputesHighTimesLow()
    {
        var inputs = new[]
        {
            ExtraIndicatorTestData.Ohlcv(10m, 20m, 10m, 15m),
            ExtraIndicatorTestData.Ohlcv(30m, 50m, 30m, 40m),
        };

        var results = await ExtraIndicatorTestData.RunAsync(MathOperators.Mult(10, "MULT"), inputs);

        Assert.Equal(200m, results[0].Indicators["MULT"]);
        Assert.Equal(1500m, results[1].Indicators["MULT"]);
    }

    [Fact]
    public async Task Div_ComputesHighDividedByLow()
    {
        var inputs = new[]
        {
            ExtraIndicatorTestData.Ohlcv(10m, 20m, 10m, 15m),
            ExtraIndicatorTestData.Ohlcv(30m, 50m, 25m, 40m),
        };

        var results = await ExtraIndicatorTestData.RunAsync(MathOperators.Div(10, "DIV"), inputs);

        Assert.Equal(2m, results[0].Indicators["DIV"]);
        Assert.Equal(2m, results[1].Indicators["DIV"]);
    }

    [Fact]
    public async Task Div_SkipsBarsWithZeroLow()
    {
        var inputs = new[]
        {
            ExtraIndicatorTestData.Ohlcv(10m, 20m, 10m, 15m),
            ExtraIndicatorTestData.Ohlcv(30m, 50m, 0m, 40m),
            ExtraIndicatorTestData.Ohlcv(30m, 60m, 30m, 40m),
        };

        var results = await ExtraIndicatorTestData.RunAsync(MathOperators.Div(10, "DIV"), inputs);

        Assert.Equal(2m, results[0].Indicators["DIV"]);
        Assert.False(results[1].Indicators.ContainsKey("DIV"));
        Assert.Equal(2m, results[2].Indicators["DIV"]);
    }

    [Fact]
    public async Task CumSum_AccumulatesClosingPricesAcrossBars()
    {
        var inputs = ExtraIndicatorTestData.Candles(new[] { 10m, 20m, 30m });

        var results = await ExtraIndicatorTestData.RunAsync(MathOperators.CumSum(14, "CUM"), inputs);

        Assert.Equal(10m, results[0].Indicators["CUM"]);
        Assert.Equal(30m, results[1].Indicators["CUM"]);
        Assert.Equal(60m, results[2].Indicators["CUM"]);
    }

    [Fact]
    public async Task Factories_UseMethodNameAsDefaultKey()
    {
        var factories = new (string Key, Func<TransformBlock<EnrichedCandle, EnrichedCandle>> Create)[]
        {
            ("Add", () => MathOperators.Add(14)),
            ("Sub", () => MathOperators.Sub(14)),
            ("Mult", () => MathOperators.Mult(14)),
            ("Div", () => MathOperators.Div(14)),
            ("CumSum", () => MathOperators.CumSum(14)),
        };

        foreach (var factory in factories)
        {
            var results = await ExtraIndicatorTestData.RunAsync(
                factory.Create(),
                new[] { ExtraIndicatorTestData.Ohlcv(10m, 20m, 10m, 15m) });

            Assert.True(results[0].Indicators.ContainsKey(factory.Key));
        }
    }

    [Fact]
    public void Factories_RejectNonPositivePeriods()
    {
        var factories = new Action<int>[]
        {
            period => MathOperators.Add(period),
            period => MathOperators.Sub(period),
            period => MathOperators.Mult(period),
            period => MathOperators.Div(period),
            period => MathOperators.CumSum(period),
        };

        foreach (var factory in factories)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => factory(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => factory(-1));
        }
    }

    [Fact]
    public void Lookbacks_AreCallableWithoutArguments()
    {
        Assert.Equal(0, MathOperators.AddLookback());
        Assert.Equal(0, MathOperators.SubLookback());
        Assert.Equal(0, MathOperators.MultLookback());
        Assert.Equal(0, MathOperators.DivLookback());
        Assert.Equal(0, MathOperators.CumSumLookback());
    }

    [Fact]
    public async Task Operations_DoNotMutateCandle()
    {
        var input = ExtraIndicatorTestData.Ohlcv(10m, 20m, 15m, 18m, 250L);

        var results = await ExtraIndicatorTestData.RunAsync(
            MathOperators.Add(14, "ADD"),
            new[] { input });

        Assert.Equal(10m, results[0].Candle.Open);
        Assert.Equal(20m, results[0].Candle.High);
        Assert.Equal(15m, results[0].Candle.Low);
        Assert.Equal(18m, results[0].Candle.Close);
        Assert.Equal(250L, results[0].Candle.Volume);
    }
}
