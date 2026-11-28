using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;
using OpenQuant.Models;
using Xunit;

namespace OpenQuant.Tests.Analysis;

public class CrossCheckTests
{
    private const int DataCount = 10000;
    private static readonly DateTimeOffset BaseTime = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Indicators_MatchReferenceImplementation()
    {
        // Generate a deterministic price series
        var candles = GenerateCandles();

        // Run library indicators
        var smaValues = ProcessSingleKeyIndicator(MovingAverage.SMA("SMA", 14), candles, "SMA");
        var emaValues = ProcessSingleKeyIndicator(MovingAverage.EMA("EMA", 14), candles, "EMA");
        var bbEnriched = ProcessFullBlock(VolatilityIndicators.BBands("BB", 20, 2m), candles);
        var stochEnriched = ProcessFullBlock(StochasticIndicators.Stoch("Stoch", 14, 3, 3), candles);
        var aroonEnriched = ProcessFullBlock(MomentumOscillators.Aroon("Aroon", 14), candles);
        var aroonOscValues = ProcessSingleKeyIndicator(MomentumOscillators.AroonOsc("AroonOsc", 14), candles, "AroonOsc");

        var bbUpper = Extract(bbEnriched, "BB_Upper");
        var bbMiddle = Extract(bbEnriched, "BB_Middle");
        var bbLower = Extract(bbEnriched, "BB_Lower");
        var percentB = Extract(bbEnriched, "BB_PercentB");
        var stochK = Extract(stochEnriched, "Stoch_K");
        var stochD = Extract(stochEnriched, "Stoch_D");
        var aroonUp = Extract(aroonEnriched, "Aroon_Up");
        var aroonDown = Extract(aroonEnriched, "Aroon_Down");

        // Write CSV file for reference comparison
        var csvPath = WriteCsv(
            candles,
            smaValues,
            emaValues,
            bbUpper,
            bbMiddle,
            bbLower,
            percentB,
            stochK,
            stochD,
            aroonUp,
            aroonDown,
            aroonOscValues);

        // Run Python reference script and assert no mismatches
        var exitCode = RunReferenceScript(csvPath);
        Assert.Equal(0, exitCode);
    }

    private static List<Candle> GenerateCandles()
    {
        var rng = new Random(42);
        var list = new List<Candle>(DataCount);
        decimal price = 100m;
        for (int i = 0; i < DataCount; i++)
        {
            var open = price;
            var step = (decimal)((rng.NextDouble() * 2.0) - 1.0);
            var close = open + step;
            var high = Math.Max(open, close) + (decimal)(rng.NextDouble() * 0.5);
            var low = Math.Min(open, close) - (decimal)(rng.NextDouble() * 0.5);
            if (low < 0)
            {
                low = 0;
            }

            var volume = rng.Next(1000, 10000);
            list.Add(new Candle
            {
                Timestamp = BaseTime.AddMinutes(i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = volume,
            });
            price = close;
        }

        return list;
    }

    private static List<decimal?> ProcessSingleKeyIndicator(TransformBlock<EnrichedCandle, EnrichedCandle> block, List<Candle> candles, string key)
    {
        var results = new List<EnrichedCandle>();
        var sink = new ActionBlock<EnrichedCandle>(c => results.Add(c));
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });
        foreach (var c in candles)
        {
            block.SendAsync(new EnrichedCandle(c)).GetAwaiter().GetResult();
        }

        block.Complete();
        sink.Completion.GetAwaiter().GetResult();
        return results.Select(r => r.Indicators.TryGetValue(key, out var v) ? (decimal?)v : null).ToList();
    }

    private static List<EnrichedCandle> ProcessFullBlock(TransformBlock<EnrichedCandle, EnrichedCandle> block, List<Candle> candles)
    {
        var outList = new List<EnrichedCandle>();
        var sink = new ActionBlock<EnrichedCandle>(c => outList.Add(c));
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });
        foreach (var c in candles)
        {
            block.SendAsync(new EnrichedCandle(c)).GetAwaiter().GetResult();
        }

        block.Complete();
        sink.Completion.GetAwaiter().GetResult();
        return outList;
    }

    private static List<decimal?> Extract(List<EnrichedCandle> enriched, string key)
    {
        return enriched.Select(c => c.Indicators.TryGetValue(key, out var v) ? (decimal?)v : null).ToList();
    }

    private static string WriteCsv(
        List<Candle> candles,
        List<decimal?> sma,
        List<decimal?> ema,
        List<decimal?> bbUpper,
        List<decimal?> bbMiddle,
        List<decimal?> bbLower,
        List<decimal?> percentB,
        List<decimal?> stochK,
        List<decimal?> stochD,
        List<decimal?> aroonUp,
        List<decimal?> aroonDown,
        List<decimal?> aroonOsc)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "OpenQuantCrossCheck");
        Directory.CreateDirectory(tempDir);
        var csvPath = Path.Combine(tempDir, "data.csv");
        using var writer = new StreamWriter(csvPath);
        var header = string.Join(
            ",",
            "Timestamp",
            "Open",
            "High",
            "Low",
            "Close",
            "Volume",
            "SMA_Lib",
            "EMA_Lib",
            "BBUpper_Lib",
            "BBMiddle_Lib",
            "BBLower_Lib",
            "PercentB_Lib",
            "StochK_Lib",
            "StochD_Lib",
            "AroonUp_Lib",
            "AroonDown_Lib",
            "AroonOsc_Lib");
        writer.WriteLine(header);
        for (int i = 0; i < candles.Count; i++)
        {
            var line = string.Join(
                ",",
                candles[i].Timestamp.ToString("o"),
                candles[i].Open.ToString(CultureInfo.InvariantCulture),
                candles[i].High.ToString(CultureInfo.InvariantCulture),
                candles[i].Low.ToString(CultureInfo.InvariantCulture),
                candles[i].Close.ToString(CultureInfo.InvariantCulture),
                candles[i].Volume.ToString(CultureInfo.InvariantCulture),
                FormatDecimal(sma[i]),
                FormatDecimal(ema[i]),
                FormatDecimal(bbUpper[i]),
                FormatDecimal(bbMiddle[i]),
                FormatDecimal(bbLower[i]),
                FormatDecimal(percentB[i]),
                FormatDecimal(stochK[i]),
                FormatDecimal(stochD[i]),
                FormatDecimal(aroonUp[i]),
                FormatDecimal(aroonDown[i]),
                FormatDecimal(aroonOsc[i]));
            writer.WriteLine(line);
        }

        return csvPath;
    }

    private static int RunReferenceScript(string csvPath)
    {
        // Resolve the absolute path to the Python reference script, which resides at the repository root.
        // The test binary runs from bin/Debug/...; navigate up to the solution directory.
        var scriptPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "cross_check_ref.py"));
        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "python",
            Arguments = $"\"{scriptPath}\" \"{csvPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = Directory.GetCurrentDirectory(),
        };
        using var proc = System.Diagnostics.Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start python process");
        proc.WaitForExit();

        // Optionally capture output for debugging (not used in test assertions)
        return proc.ExitCode;
    }

    private static string FormatDecimal(decimal? value)
    {
        return value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
    }
}
