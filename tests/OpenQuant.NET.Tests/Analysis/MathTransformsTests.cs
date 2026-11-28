using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;
using OpenQuant.Models;

namespace OpenQuant.Tests.Analysis;

/// <summary>
/// Tests for the close-price mathematical transform factories.
/// </summary>
public class MathTransformsTests
{
    [Fact]
    public async Task Factories_ComputeExpectedValuesAndUseMethodNameAsKey()
    {
        const decimal tolerance = 0.0001m;
        var cases = new (string Key, decimal Close, decimal Expected, Func<TransformBlock<EnrichedCandle, EnrichedCandle>> Create)[]
        {
            ("Acos", 1m, 0m, () => MathTransforms.Acos(14)),
            ("Asin", 0m, 0m, () => MathTransforms.Asin(14)),
            ("Atan", 0m, 0m, () => MathTransforms.Atan(14)),
            ("Ceil", 3.2m, 4m, () => MathTransforms.Ceil(14)),
            ("Cos", 0m, 1m, () => MathTransforms.Cos(14)),
            ("Cosh", 0m, 1m, () => MathTransforms.Cosh(14)),
            ("Exp", 0m, 1m, () => MathTransforms.Exp(14)),
            ("Floor", 3.7m, 3m, () => MathTransforms.Floor(14)),
            ("Ln", 1m, 0m, () => MathTransforms.Ln(14)),
            ("Log10", 1m, 0m, () => MathTransforms.Log10(14)),
            ("Sin", 0m, 0m, () => MathTransforms.Sin(14)),
            ("Sinh", 0m, 0m, () => MathTransforms.Sinh(14)),
            ("Sqrt", 4m, 2m, () => MathTransforms.Sqrt(14)),
            ("Tan", 0m, 0m, () => MathTransforms.Tan(14)),
            ("Tanh", 0m, 0m, () => MathTransforms.Tanh(14)),
        };

        foreach (var testCase in cases)
        {
            var results = await ExtraIndicatorTestData.RunAsync(
                testCase.Create(),
                new[] { ExtraIndicatorTestData.Candle(testCase.Close) });

            var value = results[0].Indicators[testCase.Key];
            Assert.InRange(value, testCase.Expected - tolerance, testCase.Expected + tolerance);
        }
    }

    [Fact]
    public async Task CustomName_IsUsedAsIndicatorKey()
    {
        var results = await ExtraIndicatorTestData.RunAsync(
            MathTransforms.Acos(14, "ARCCOS"),
            new[] { ExtraIndicatorTestData.Candle(1m) });

        Assert.Equal(0m, results[0].Indicators["ARCCOS"]);
    }

    [Fact]
    public async Task DomainFunctions_SkipOutsideDomainInputs()
    {
        var cases = new (Func<TransformBlock<EnrichedCandle, EnrichedCandle>> Create, decimal Close)[]
        {
            (() => MathTransforms.Acos(14), 1.5m),
            (() => MathTransforms.Asin(14), -1.5m),
            (() => MathTransforms.Ln(14), 0m),
            (() => MathTransforms.Log10(14), -1m),
            (() => MathTransforms.Sqrt(14), -1m),
        };

        foreach (var testCase in cases)
        {
            var results = await ExtraIndicatorTestData.RunAsync(
                testCase.Create(),
                new[] { ExtraIndicatorTestData.Candle(testCase.Close) });

            Assert.Empty(results[0].Indicators);
        }
    }

    [Fact]
    public async Task Exp_SkipsOverflowValues()
    {
        var results = await ExtraIndicatorTestData.RunAsync(
            MathTransforms.Exp(14, "EXP"),
            new[] { ExtraIndicatorTestData.Candle(1000m) });

        Assert.False(results[0].Indicators.ContainsKey("EXP"));
    }

    [Fact]
    public void Factories_RejectNonPositivePeriods()
    {
        var factories = new Func<int, TransformBlock<EnrichedCandle, EnrichedCandle>>[]
        {
            period => MathTransforms.Acos(period),
            period => MathTransforms.Asin(period),
            period => MathTransforms.Atan(period),
            period => MathTransforms.Ceil(period),
            period => MathTransforms.Cos(period),
            period => MathTransforms.Cosh(period),
            period => MathTransforms.Exp(period),
            period => MathTransforms.Floor(period),
            period => MathTransforms.Ln(period),
            period => MathTransforms.Log10(period),
            period => MathTransforms.Sin(period),
            period => MathTransforms.Sinh(period),
            period => MathTransforms.Sqrt(period),
            period => MathTransforms.Tan(period),
            period => MathTransforms.Tanh(period),
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
        Assert.Equal(0, MathTransforms.AcosLookback());
        Assert.Equal(0, MathTransforms.AsinLookback());
        Assert.Equal(0, MathTransforms.AtanLookback());
        Assert.Equal(0, MathTransforms.CeilLookback());
        Assert.Equal(0, MathTransforms.CosLookback());
        Assert.Equal(0, MathTransforms.CoshLookback());
        Assert.Equal(0, MathTransforms.ExpLookback());
        Assert.Equal(0, MathTransforms.FloorLookback());
        Assert.Equal(0, MathTransforms.LnLookback());
        Assert.Equal(0, MathTransforms.Log10Lookback());
        Assert.Equal(0, MathTransforms.SinLookback());
        Assert.Equal(0, MathTransforms.SinhLookback());
        Assert.Equal(0, MathTransforms.SqrtLookback());
        Assert.Equal(0, MathTransforms.TanLookback());
        Assert.Equal(0, MathTransforms.TanhLookback());
    }
}
