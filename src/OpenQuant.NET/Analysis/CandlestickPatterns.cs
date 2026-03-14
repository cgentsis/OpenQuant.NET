using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// Provides candlestick pattern recognition as TPL Dataflow
/// <see cref="TransformBlock{TInput,TOutput}"/> stages. Each factory returns a block that
/// detects a specific candle pattern and stores a value of <c>100</c> (bullish),
/// <c>−100</c> (bearish), or omits the key (no pattern detected).
/// </summary>
public static class CandlestickPatterns
{
    /// <summary>Detects a Doji pattern (open ≈ close, with shadows).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Doji(
        string name,
        CancellationToken cancellationToken = default)
    {
        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;
                var body = Math.Abs(c.Close - c.Open);
                var range = c.High - c.Low;

                if (range > 0m && body <= (range * 0.05m))
                {
                    enriched.Indicators[name] = 100m;
                }

                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Dragonfly Doji (open ≈ close ≈ high, long lower shadow).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> DragonflyDoji(
        string name,
        CancellationToken cancellationToken = default)
    {
        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;
                var body = Math.Abs(c.Close - c.Open);
                var range = c.High - c.Low;
                var upperShadow = c.High - Math.Max(c.Open, c.Close);

                if (range > 0m && body <= (range * 0.05m) && upperShadow <= (range * 0.05m))
                {
                    enriched.Indicators[name] = 100m;
                }

                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Gravestone Doji (open ≈ close ≈ low, long upper shadow).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> GravestoneDoji(
        string name,
        CancellationToken cancellationToken = default)
    {
        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;
                var body = Math.Abs(c.Close - c.Open);
                var range = c.High - c.Low;
                var lowerShadow = Math.Min(c.Open, c.Close) - c.Low;

                if (range > 0m && body <= (range * 0.05m) && lowerShadow <= (range * 0.05m))
                {
                    enriched.Indicators[name] = -100m;
                }

                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Long-Legged Doji (open ≈ close near middle, long shadows both sides).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> LongLeggedDoji(
        string name,
        CancellationToken cancellationToken = default)
    {
        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;
                var body = Math.Abs(c.Close - c.Open);
                var range = c.High - c.Low;
                var upper = c.High - Math.Max(c.Open, c.Close);
                var lower = Math.Min(c.Open, c.Close) - c.Low;

                if (range > 0m && body <= (range * 0.05m) && upper >= (range * 0.3m) && lower >= (range * 0.3m))
                {
                    enriched.Indicators[name] = 100m;
                }

                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Hammer (small body at top, long lower shadow, bullish reversal).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Hammer(
        string name,
        CancellationToken cancellationToken = default)
    {
        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;
                var body = Math.Abs(c.Close - c.Open);
                var range = c.High - c.Low;
                var lowerShadow = Math.Min(c.Open, c.Close) - c.Low;
                var upperShadow = c.High - Math.Max(c.Open, c.Close);

                if (range > 0m && body > 0m && lowerShadow >= (2m * body) && upperShadow <= (body * 0.3m))
                {
                    enriched.Indicators[name] = 100m;
                }

                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects an Inverted Hammer (small body at bottom, long upper shadow).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> InvertedHammer(
        string name,
        CancellationToken cancellationToken = default)
    {
        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;
                var body = Math.Abs(c.Close - c.Open);
                var range = c.High - c.Low;
                var upperShadow = c.High - Math.Max(c.Open, c.Close);
                var lowerShadow = Math.Min(c.Open, c.Close) - c.Low;

                if (range > 0m && body > 0m && upperShadow >= (2m * body) && lowerShadow <= (body * 0.3m))
                {
                    enriched.Indicators[name] = 100m;
                }

                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Hanging Man (same shape as hammer but in uptrend — bearish).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> HangingMan(
        string name,
        CancellationToken cancellationToken = default)
    {
        decimal? prevClose = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;
                var body = Math.Abs(c.Close - c.Open);
                var lowerShadow = Math.Min(c.Open, c.Close) - c.Low;
                var upperShadow = c.High - Math.Max(c.Open, c.Close);
                var range = c.High - c.Low;

                if (prevClose.HasValue && c.Open > prevClose.Value &&
                    range > 0m && body > 0m && lowerShadow >= (2m * body) && upperShadow <= (body * 0.3m))
                {
                    enriched.Indicators[name] = -100m;
                }

                prevClose = c.Close;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Shooting Star (inverted hammer shape after uptrend — bearish).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> ShootingStar(
        string name,
        CancellationToken cancellationToken = default)
    {
        decimal? prevClose = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;
                var body = Math.Abs(c.Close - c.Open);
                var upperShadow = c.High - Math.Max(c.Open, c.Close);
                var lowerShadow = Math.Min(c.Open, c.Close) - c.Low;
                var range = c.High - c.Low;

                if (prevClose.HasValue && c.Open > prevClose.Value &&
                    range > 0m && body > 0m && upperShadow >= (2m * body) && lowerShadow <= (body * 0.3m))
                {
                    enriched.Indicators[name] = -100m;
                }

                prevClose = c.Close;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Marubozu (no shadows, strong directional bar).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Marubozu(
        string name,
        CancellationToken cancellationToken = default)
    {
        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;
                var body = Math.Abs(c.Close - c.Open);
                var range = c.High - c.Low;

                if (range > 0m && body >= (range * 0.95m))
                {
                    enriched.Indicators[name] = c.Close > c.Open ? 100m : -100m;
                }

                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Closing Marubozu (close equals high for bullish, close equals low for bearish).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> ClosingMarubozu(
        string name,
        CancellationToken cancellationToken = default)
    {
        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;
                var body = Math.Abs(c.Close - c.Open);
                var range = c.High - c.Low;

                if (range > 0m && body >= (range * 0.6m))
                {
                    if (c.Close > c.Open && c.Close == c.High)
                    {
                        enriched.Indicators[name] = 100m;
                    }
                    else if (c.Close < c.Open && c.Close == c.Low)
                    {
                        enriched.Indicators[name] = -100m;
                    }
                }

                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Spinning Top (small body, roughly equal upper and lower shadows).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> SpinningTop(
        string name,
        CancellationToken cancellationToken = default)
    {
        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;
                var body = Math.Abs(c.Close - c.Open);
                var range = c.High - c.Low;
                var upper = c.High - Math.Max(c.Open, c.Close);
                var lower = Math.Min(c.Open, c.Close) - c.Low;

                if (range > 0m && body <= (range * 0.3m) && upper >= (range * 0.2m) && lower >= (range * 0.2m))
                {
                    enriched.Indicators[name] = 100m;
                }

                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Bullish Engulfing pattern (current bullish candle engulfs previous bearish).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Engulfing(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev != null)
                {
                    if (prev.Close < prev.Open && c.Close > c.Open &&
                        c.Open <= prev.Close && c.Close >= prev.Open)
                    {
                        enriched.Indicators[name] = 100m;
                    }
                    else if (prev.Close > prev.Open && c.Close < c.Open &&
                             c.Open >= prev.Close && c.Close <= prev.Open)
                    {
                        enriched.Indicators[name] = -100m;
                    }
                }

                prev = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Harami pattern (current small body within previous large body).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Harami(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev != null)
                {
                    var prevBody = Math.Abs(prev.Close - prev.Open);
                    var curBody = Math.Abs(c.Close - c.Open);

                    if (curBody < prevBody)
                    {
                        var prevHigh = Math.Max(prev.Open, prev.Close);
                        var prevLow = Math.Min(prev.Open, prev.Close);
                        var curHigh = Math.Max(c.Open, c.Close);
                        var curLow = Math.Min(c.Open, c.Close);

                        if (curHigh <= prevHigh && curLow >= prevLow)
                        {
                            if (prev.Close < prev.Open && c.Close > c.Open)
                            {
                                enriched.Indicators[name] = 100m;
                            }
                            else if (prev.Close > prev.Open && c.Close < c.Open)
                            {
                                enriched.Indicators[name] = -100m;
                            }
                        }
                    }
                }

                prev = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Harami Cross (Harami where the second candle is a Doji).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> HaramiCross(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev != null)
                {
                    var curBody = Math.Abs(c.Close - c.Open);
                    var curRange = c.High - c.Low;
                    var prevHighBody = Math.Max(prev.Open, prev.Close);
                    var prevLowBody = Math.Min(prev.Open, prev.Close);

                    if (curRange > 0m && curBody <= (curRange * 0.05m) &&
                        c.High <= prevHighBody && c.Low >= prevLowBody)
                    {
                        enriched.Indicators[name] = prev.Close < prev.Open ? 100m : -100m;
                    }
                }

                prev = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Piercing Line (bullish reversal: gap down then close above midpoint of prior).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Piercing(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev != null && prev.Close < prev.Open && c.Close > c.Open &&
                    c.Open < prev.Close && c.Close > ((prev.Open + prev.Close) / 2m) && c.Close < prev.Open)
                {
                    enriched.Indicators[name] = 100m;
                }

                prev = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Dark Cloud Cover (bearish reversal: gap up then close below midpoint of prior).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> DarkCloudCover(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev != null && prev.Close > prev.Open && c.Close < c.Open &&
                    c.Open > prev.Close && c.Close < ((prev.Open + prev.Close) / 2m) && c.Close > prev.Open)
                {
                    enriched.Indicators[name] = -100m;
                }

                prev = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Morning Star pattern (3-bar bullish reversal).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> MorningStar(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev2 = null;
        Candle? prev1 = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev2 != null && prev1 != null)
                {
                    var body2 = Math.Abs(prev2.Close - prev2.Open);
                    var body1 = Math.Abs(prev1.Close - prev1.Open);
                    var range2 = prev2.High - prev2.Low;

                    if (prev2.Close < prev2.Open && range2 > 0m && body2 >= (range2 * 0.5m) &&
                        body1 < body2 && c.Close > c.Open &&
                        c.Close > ((prev2.Open + prev2.Close) / 2m))
                    {
                        enriched.Indicators[name] = 100m;
                    }
                }

                prev2 = prev1;
                prev1 = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects an Evening Star pattern (3-bar bearish reversal).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> EveningStar(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev2 = null;
        Candle? prev1 = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev2 != null && prev1 != null)
                {
                    var body2 = Math.Abs(prev2.Close - prev2.Open);
                    var body1 = Math.Abs(prev1.Close - prev1.Open);
                    var range2 = prev2.High - prev2.Low;

                    if (prev2.Close > prev2.Open && range2 > 0m && body2 >= (range2 * 0.5m) &&
                        body1 < body2 && c.Close < c.Open &&
                        c.Close < ((prev2.Open + prev2.Close) / 2m))
                    {
                        enriched.Indicators[name] = -100m;
                    }
                }

                prev2 = prev1;
                prev1 = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Morning Doji Star (3-bar pattern with a Doji in the middle, bullish).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> MorningDojiStar(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev2 = null;
        Candle? prev1 = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev2 != null && prev1 != null)
                {
                    var body2 = Math.Abs(prev2.Close - prev2.Open);
                    var range2 = prev2.High - prev2.Low;
                    var body1 = Math.Abs(prev1.Close - prev1.Open);
                    var range1 = prev1.High - prev1.Low;

                    if (prev2.Close < prev2.Open && range2 > 0m && body2 >= (range2 * 0.5m) &&
                        range1 > 0m && body1 <= (range1 * 0.05m) &&
                        c.Close > c.Open && c.Close > ((prev2.Open + prev2.Close) / 2m))
                    {
                        enriched.Indicators[name] = 100m;
                    }
                }

                prev2 = prev1;
                prev1 = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects an Evening Doji Star (3-bar pattern with a Doji in the middle, bearish).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> EveningDojiStar(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev2 = null;
        Candle? prev1 = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev2 != null && prev1 != null)
                {
                    var body2 = Math.Abs(prev2.Close - prev2.Open);
                    var range2 = prev2.High - prev2.Low;
                    var body1 = Math.Abs(prev1.Close - prev1.Open);
                    var range1 = prev1.High - prev1.Low;

                    if (prev2.Close > prev2.Open && range2 > 0m && body2 >= (range2 * 0.5m) &&
                        range1 > 0m && body1 <= (range1 * 0.05m) &&
                        c.Close < c.Open && c.Close < ((prev2.Open + prev2.Close) / 2m))
                    {
                        enriched.Indicators[name] = -100m;
                    }
                }

                prev2 = prev1;
                prev1 = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects Three White Soldiers (3 consecutive bullish candles with higher closes).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> ThreeWhiteSoldiers(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev2 = null;
        Candle? prev1 = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev2 != null && prev1 != null &&
                    prev2.Close > prev2.Open && prev1.Close > prev1.Open && c.Close > c.Open &&
                    prev1.Close > prev2.Close && c.Close > prev1.Close &&
                    prev1.Open > prev2.Open && c.Open > prev1.Open)
                {
                    enriched.Indicators[name] = 100m;
                }

                prev2 = prev1;
                prev1 = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects Three Black Crows (3 consecutive bearish candles with lower closes).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> ThreeBlackCrows(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev2 = null;
        Candle? prev1 = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev2 != null && prev1 != null &&
                    prev2.Close < prev2.Open && prev1.Close < prev1.Open && c.Close < c.Open &&
                    prev1.Close < prev2.Close && c.Close < prev1.Close &&
                    prev1.Open < prev2.Open && c.Open < prev1.Open)
                {
                    enriched.Indicators[name] = -100m;
                }

                prev2 = prev1;
                prev1 = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Long Line Candle (large body relative to range).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> LongLine(
        string name,
        CancellationToken cancellationToken = default)
    {
        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;
                var body = Math.Abs(c.Close - c.Open);
                var range = c.High - c.Low;

                if (range > 0m && body >= (range * 0.7m))
                {
                    enriched.Indicators[name] = c.Close > c.Open ? 100m : -100m;
                }

                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Short Line Candle (small body relative to range).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> ShortLine(
        string name,
        CancellationToken cancellationToken = default)
    {
        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;
                var body = Math.Abs(c.Close - c.Open);
                var range = c.High - c.Low;

                if (range > 0m && body <= (range * 0.25m))
                {
                    enriched.Indicators[name] = c.Close >= c.Open ? 100m : -100m;
                }

                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Doji Star (Doji following a long candle, potential reversal).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> DojiStar(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev != null)
                {
                    var prevBody = Math.Abs(prev.Close - prev.Open);
                    var prevRange = prev.High - prev.Low;
                    var curBody = Math.Abs(c.Close - c.Open);
                    var curRange = c.High - c.Low;

                    if (prevRange > 0m && prevBody >= (prevRange * 0.5m) &&
                        curRange > 0m && curBody <= (curRange * 0.05m))
                    {
                        enriched.Indicators[name] = prev.Close > prev.Open ? -100m : 100m;
                    }
                }

                prev = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Belt Hold (strong gap open in trend direction with no shadow on open side).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> BeltHold(
        string name,
        CancellationToken cancellationToken = default)
    {
        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;
                var body = Math.Abs(c.Close - c.Open);
                var range = c.High - c.Low;

                if (range > 0m && body >= (range * 0.6m))
                {
                    if (c.Close > c.Open && c.Open == c.Low)
                    {
                        enriched.Indicators[name] = 100m;
                    }
                    else if (c.Close < c.Open && c.Open == c.High)
                    {
                        enriched.Indicators[name] = -100m;
                    }
                }

                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Kicking pattern (Marubozu gap in opposite direction).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Kicking(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev != null)
                {
                    var prevBody = Math.Abs(prev.Close - prev.Open);
                    var prevRange = prev.High - prev.Low;
                    var curBody = Math.Abs(c.Close - c.Open);
                    var curRange = c.High - c.Low;

                    var prevMarubozu = prevRange > 0m && prevBody >= (prevRange * 0.95m);
                    var curMarubozu = curRange > 0m && curBody >= (curRange * 0.95m);

                    if (prevMarubozu && curMarubozu)
                    {
                        if (prev.Close < prev.Open && c.Close > c.Open && c.Low > prev.High)
                        {
                            enriched.Indicators[name] = 100m;
                        }
                        else if (prev.Close > prev.Open && c.Close < c.Open && c.High < prev.Low)
                        {
                            enriched.Indicators[name] = -100m;
                        }
                    }
                }

                prev = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Matching Low (two candles with the same close, bullish reversal).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> MatchingLow(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev != null && prev.Close < prev.Open && c.Close < c.Open && c.Close == prev.Close)
                {
                    enriched.Indicators[name] = 100m;
                }

                prev = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Homing Pigeon (Harami where both candles are bearish).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> HomingPigeon(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev != null && prev.Close < prev.Open && c.Close < c.Open &&
                    c.Open <= prev.Open && c.Close >= prev.Close)
                {
                    enriched.Indicators[name] = 100m;
                }

                prev = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Counterattack (two opposite candles closing at the same price).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Counterattack(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev != null && Math.Abs(c.Close - prev.Close) <= ((c.High - c.Low) * 0.01m))
                {
                    if (prev.Close < prev.Open && c.Close > c.Open)
                    {
                        enriched.Indicators[name] = 100m;
                    }
                    else if (prev.Close > prev.Open && c.Close < c.Open)
                    {
                        enriched.Indicators[name] = -100m;
                    }
                }

                prev = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Rickshaw Man (Doji with long and roughly equal shadows, at the midpoint).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> RickshawMan(
        string name,
        CancellationToken cancellationToken = default)
    {
        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;
                var body = Math.Abs(c.Close - c.Open);
                var range = c.High - c.Low;
                var mid = (c.High + c.Low) / 2m;
                var bodyMid = (c.Open + c.Close) / 2m;

                if (range > 0m && body <= (range * 0.05m) && Math.Abs(bodyMid - mid) <= (range * 0.1m))
                {
                    enriched.Indicators[name] = 100m;
                }

                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a High-Wave Candle (small body with very long shadows on both sides).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> HighWave(
        string name,
        CancellationToken cancellationToken = default)
    {
        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;
                var body = Math.Abs(c.Close - c.Open);
                var range = c.High - c.Low;
                var upper = c.High - Math.Max(c.Open, c.Close);
                var lower = Math.Min(c.Open, c.Close) - c.Low;

                if (range > 0m && body <= (range * 0.15m) && upper >= (range * 0.35m) && lower >= (range * 0.35m))
                {
                    enriched.Indicators[name] = 100m;
                }

                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Tristar pattern (3 consecutive Doji candles).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Tristar(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev2 = null;
        Candle? prev1 = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev2 != null && prev1 != null)
                {
                    var isDoji = (Candle cd) =>
                    {
                        var b = Math.Abs(cd.Close - cd.Open);
                        var r = cd.High - cd.Low;
                        return r > 0m && b <= (r * 0.05m);
                    };

                    if (isDoji(prev2) && isDoji(prev1) && isDoji(c))
                    {
                        var mid1 = (prev1.Open + prev1.Close) / 2m;
                        var mid2 = (prev2.Open + prev2.Close) / 2m;
                        enriched.Indicators[name] = mid1 < mid2 ? 100m : -100m;
                    }
                }

                prev2 = prev1;
                prev1 = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Stick Sandwich (bearish, bullish, bearish with same close as first).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> StickSandwich(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev2 = null;
        Candle? prev1 = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev2 != null && prev1 != null &&
                    prev2.Close < prev2.Open && prev1.Close > prev1.Open && c.Close < c.Open &&
                    c.Close == prev2.Close)
                {
                    enriched.Indicators[name] = 100m;
                }

                prev2 = prev1;
                prev1 = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Thrusting Pattern (bearish continuation: gap down, close into prior body but below midpoint).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Thrusting(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev != null && prev.Close < prev.Open && c.Close > c.Open &&
                    c.Open < prev.Close && c.Close < ((prev.Open + prev.Close) / 2m) && c.Close > prev.Close)
                {
                    enriched.Indicators[name] = -100m;
                }

                prev = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects an In-Neck Pattern (bearish continuation: close near prior close from below).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> InNeck(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev != null && prev.Close < prev.Open && c.Close > c.Open && c.Open < prev.Close)
                {
                    var tolerance = (prev.High - prev.Low) * 0.03m;

                    if (Math.Abs(c.Close - prev.Close) <= tolerance)
                    {
                        enriched.Indicators[name] = -100m;
                    }
                }

                prev = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects an On-Neck Pattern (bearish continuation: close equals prior low).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> OnNeck(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev != null && prev.Close < prev.Open && c.Close > c.Open && c.Open < prev.Close)
                {
                    var tolerance = (prev.High - prev.Low) * 0.03m;

                    if (Math.Abs(c.Close - prev.Low) <= tolerance)
                    {
                        enriched.Indicators[name] = -100m;
                    }
                }

                prev = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Separating Lines pattern (same open direction but opposite candle).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> SeparatingLines(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev != null)
                {
                    var tolerance = (c.High - c.Low) * 0.01m;
                    var sameOpen = Math.Abs(c.Open - prev.Open) <= tolerance;

                    if (sameOpen && prev.Close < prev.Open && c.Close > c.Open)
                    {
                        enriched.Indicators[name] = 100m;
                    }
                    else if (sameOpen && prev.Close > prev.Open && c.Close < c.Open)
                    {
                        enriched.Indicators[name] = -100m;
                    }
                }

                prev = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Two Crows pattern (3-bar bearish reversal).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> TwoCrows(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev2 = null;
        Candle? prev1 = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev2 != null && prev1 != null)
                {
                    var firstBody = Math.Abs(prev2.Close - prev2.Open);
                    var firstRange = prev2.High - prev2.Low;

                    if (firstRange > 0m && firstBody >= (firstRange * 0.5m) &&
                        prev2.Close > prev2.Open && prev1.Close < prev1.Open && c.Close < c.Open &&
                        prev1.Open > prev2.High &&
                        c.Open < prev1.Open && c.Open > prev1.Close &&
                        c.Close < prev2.Close && c.Close > prev2.Open)
                    {
                        enriched.Indicators[name] = -100m;
                    }
                }

                prev2 = prev1;
                prev1 = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Three Inside pattern (Harami with confirmation).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> ThreeInside(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev2 = null;
        Candle? prev1 = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev2 != null && prev1 != null)
                {
                    var firstBody = Math.Abs(prev2.Close - prev2.Open);
                    var secondBody = Math.Abs(prev1.Close - prev1.Open);
                    var firstBodyHigh = Math.Max(prev2.Open, prev2.Close);
                    var firstBodyLow = Math.Min(prev2.Open, prev2.Close);
                    var secondBodyHigh = Math.Max(prev1.Open, prev1.Close);
                    var secondBodyLow = Math.Min(prev1.Open, prev1.Close);
                    var insideBody = secondBodyHigh <= firstBodyHigh && secondBodyLow >= firstBodyLow && secondBody < firstBody;

                    if (insideBody)
                    {
                        if (prev2.Close < prev2.Open && prev1.Close > prev1.Open && c.Close > c.Open && c.Close > prev2.Open)
                        {
                            enriched.Indicators[name] = 100m;
                        }
                        else if (prev2.Close > prev2.Open && prev1.Close < prev1.Open && c.Close < c.Open && c.Close < prev2.Open)
                        {
                            enriched.Indicators[name] = -100m;
                        }
                    }
                }

                prev2 = prev1;
                prev1 = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Three Line Strike pattern (4-bar continuation).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> ThreeLineStrike(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev3 = null;
        Candle? prev2 = null;
        Candle? prev1 = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev3 != null && prev2 != null && prev1 != null)
                {
                    if (prev3.Close > prev3.Open && prev2.Close > prev2.Open && prev1.Close > prev1.Open &&
                        prev2.Close > prev3.Close && prev1.Close > prev2.Close &&
                        prev2.Open > prev3.Open && prev1.Open > prev2.Open &&
                        c.Close < c.Open && c.Open > prev1.Close && c.Close < prev3.Open)
                    {
                        enriched.Indicators[name] = 100m;
                    }
                    else if (prev3.Close < prev3.Open && prev2.Close < prev2.Open && prev1.Close < prev1.Open &&
                        prev2.Close < prev3.Close && prev1.Close < prev2.Close &&
                        prev2.Open < prev3.Open && prev1.Open < prev2.Open &&
                        c.Close > c.Open && c.Open < prev1.Close && c.Close > prev3.Open)
                    {
                        enriched.Indicators[name] = -100m;
                    }
                }

                prev3 = prev2;
                prev2 = prev1;
                prev1 = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Three Stars In South pattern (3-bar bullish exhaustion).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> ThreeStarsInSouth(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev2 = null;
        Candle? prev1 = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev2 != null && prev1 != null)
                {
                    var body2 = Math.Abs(prev2.Close - prev2.Open);
                    var body1 = Math.Abs(prev1.Close - prev1.Open);
                    var body0 = Math.Abs(c.Close - c.Open);
                    var upper2 = prev2.High - Math.Max(prev2.Open, prev2.Close);
                    var upper1 = prev1.High - Math.Max(prev1.Open, prev1.Close);
                    var upper0 = c.High - Math.Max(c.Open, c.Close);
                    var lower2 = Math.Min(prev2.Open, prev2.Close) - prev2.Low;
                    var lower1 = Math.Min(prev1.Open, prev1.Close) - prev1.Low;
                    var lower0 = Math.Min(c.Open, c.Close) - c.Low;

                    if (prev2.Close < prev2.Open && prev1.Close < prev1.Open && c.Close < c.Open &&
                        prev1.Close < prev2.Close && c.Close < prev1.Close &&
                        body1 < body2 && body0 < body1 &&
                        lower2 > lower1 && lower1 >= lower0 &&
                        upper2 >= upper1 && upper1 >= upper0)
                    {
                        enriched.Indicators[name] = 100m;
                    }
                }

                prev2 = prev1;
                prev1 = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects an Abandoned Baby pattern (3-bar reversal with gaps around a Doji).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> AbandonedBaby(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev2 = null;
        Candle? prev1 = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev2 != null && prev1 != null)
                {
                    var firstBody = Math.Abs(prev2.Close - prev2.Open);
                    var firstRange = prev2.High - prev2.Low;
                    var dojiBody = Math.Abs(prev1.Close - prev1.Open);
                    var dojiRange = prev1.High - prev1.Low;
                    var isDoji = dojiRange > 0m && dojiBody <= (dojiRange * 0.05m);

                    if (firstRange > 0m && firstBody >= (firstRange * 0.5m) && isDoji)
                    {
                        if (prev2.Close < prev2.Open && prev1.High < prev2.Low && c.Close > c.Open && c.Low > prev1.High)
                        {
                            enriched.Indicators[name] = 100m;
                        }
                        else if (prev2.Close > prev2.Open && prev1.Low > prev2.High && c.Close < c.Open && c.High < prev1.Low)
                        {
                            enriched.Indicators[name] = -100m;
                        }
                    }
                }

                prev2 = prev1;
                prev1 = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects an Advance Block pattern (3 bullish candles with weakening momentum).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> AdvanceBlock(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev2 = null;
        Candle? prev1 = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev2 != null && prev1 != null)
                {
                    var body2 = Math.Abs(prev2.Close - prev2.Open);
                    var body1 = Math.Abs(prev1.Close - prev1.Open);
                    var body0 = Math.Abs(c.Close - c.Open);
                    var upper2 = prev2.High - Math.Max(prev2.Open, prev2.Close);
                    var upper1 = prev1.High - Math.Max(prev1.Open, prev1.Close);
                    var upper0 = c.High - Math.Max(c.Open, c.Close);

                    if (prev2.Close > prev2.Open && prev1.Close > prev1.Open && c.Close > c.Open &&
                        prev1.Close > prev2.Close && c.Close > prev1.Close &&
                        body2 > body1 && body1 > body0 &&
                        upper2 < upper1 && upper1 < upper0)
                    {
                        enriched.Indicators[name] = -100m;
                    }
                }

                prev2 = prev1;
                prev1 = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Breakaway pattern (5-bar reversal).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Breakaway(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev4 = null;
        Candle? prev3 = null;
        Candle? prev2 = null;
        Candle? prev1 = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev4 != null && prev3 != null && prev2 != null && prev1 != null)
                {
                    var firstBody = Math.Abs(prev4.Close - prev4.Open);
                    var firstRange = prev4.High - prev4.Low;
                    var thirdBody = Math.Abs(prev2.Close - prev2.Open);
                    var thirdRange = prev2.High - prev2.Low;
                    var fourthBody = Math.Abs(prev1.Close - prev1.Open);
                    var fourthRange = prev1.High - prev1.Low;
                    var finalBody = Math.Abs(c.Close - c.Open);
                    var finalRange = c.High - c.Low;
                    var thirdSmall = thirdRange > 0m && thirdBody <= (thirdRange * 0.4m);
                    var fourthSmall = fourthRange > 0m && fourthBody <= (fourthRange * 0.4m);

                    if (firstRange > 0m && finalRange > 0m && firstBody >= (firstRange * 0.5m) && finalBody >= (finalRange * 0.5m) && thirdSmall && fourthSmall)
                    {
                        if (prev4.Close < prev4.Open && prev3.Close < prev3.Open && prev3.High < prev4.Low &&
                            prev2.High < prev4.Low && prev1.High < prev4.Low &&
                            c.Close > c.Open && c.Close > prev3.High && c.Close < prev4.Low)
                        {
                            enriched.Indicators[name] = 100m;
                        }
                        else if (prev4.Close > prev4.Open && prev3.Close > prev3.Open && prev3.Low > prev4.High &&
                            prev2.Low > prev4.High && prev1.Low > prev4.High &&
                            c.Close < c.Open && c.Close < prev3.Low && c.Close > prev4.High)
                        {
                            enriched.Indicators[name] = -100m;
                        }
                    }
                }

                prev4 = prev3;
                prev3 = prev2;
                prev2 = prev1;
                prev1 = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Concealing Baby Swallow pattern (4-bar bullish reversal).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> ConcealingBabySwallow(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev3 = null;
        Candle? prev2 = null;
        Candle? prev1 = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev3 != null && prev2 != null && prev1 != null)
                {
                    var firstBody = Math.Abs(prev3.Close - prev3.Open);
                    var firstRange = prev3.High - prev3.Low;
                    var secondBody = Math.Abs(prev2.Close - prev2.Open);
                    var secondRange = prev2.High - prev2.Low;
                    var firstMarubozu = firstRange > 0m && firstBody >= (firstRange * 0.95m) && prev3.Close < prev3.Open;
                    var secondMarubozu = secondRange > 0m && secondBody >= (secondRange * 0.95m) && prev2.Close < prev2.Open;

                    if (firstMarubozu && secondMarubozu && prev1.Close < prev1.Open && c.Close < c.Open &&
                        prev1.Open < prev2.Close && prev1.High > prev2.Close && prev1.High < prev2.Open &&
                        c.High >= prev1.High && c.Low <= prev1.Low)
                    {
                        enriched.Indicators[name] = 100m;
                    }
                }

                prev3 = prev2;
                prev2 = prev1;
                prev1 = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Gap Side-Side White pattern (3-bar gap continuation).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> GapSideSideWhite(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev2 = null;
        Candle? prev1 = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev2 != null && prev1 != null)
                {
                    var secondBody = Math.Abs(prev1.Close - prev1.Open);
                    var thirdBody = Math.Abs(c.Close - c.Open);
                    var secondRange = prev1.High - prev1.Low;
                    var similarBody = Math.Abs(thirdBody - secondBody) <= (Math.Max(thirdBody, secondBody) * 0.1m);
                    var similarOpen = secondRange > 0m && Math.Abs(c.Open - prev1.Open) <= (secondRange * 0.1m);

                    if (similarBody && similarOpen)
                    {
                        if (prev1.Close > prev1.Open && c.Close > c.Open && prev1.Low > prev2.High && c.Low > prev2.High)
                        {
                            enriched.Indicators[name] = 100m;
                        }
                        else if (prev1.Close < prev1.Open && c.Close < c.Open && prev1.High < prev2.Low && c.High < prev2.Low)
                        {
                            enriched.Indicators[name] = -100m;
                        }
                    }
                }

                prev2 = prev1;
                prev1 = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Hikkake pattern (inside bar followed by a breakout).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Hikkake(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev2 = null;
        Candle? prev1 = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev2 != null && prev1 != null && prev1.High < prev2.High && prev1.Low > prev2.Low)
                {
                    if (c.Close > prev2.High)
                    {
                        enriched.Indicators[name] = 100m;
                    }
                    else if (c.Close < prev2.Low)
                    {
                        enriched.Indicators[name] = -100m;
                    }
                }

                prev2 = prev1;
                prev1 = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Modified Hikkake pattern (inside bar plus Harami before breakout).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> HikkakeMod(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev2 = null;
        Candle? prev1 = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev2 != null && prev1 != null)
                {
                    var firstBodyHigh = Math.Max(prev2.Open, prev2.Close);
                    var firstBodyLow = Math.Min(prev2.Open, prev2.Close);
                    var secondBodyHigh = Math.Max(prev1.Open, prev1.Close);
                    var secondBodyLow = Math.Min(prev1.Open, prev1.Close);
                    var insideRange = prev1.High < prev2.High && prev1.Low > prev2.Low;
                    var insideBody = secondBodyHigh <= firstBodyHigh && secondBodyLow >= firstBodyLow;

                    if (insideRange && insideBody)
                    {
                        if (c.Close > prev2.High)
                        {
                            enriched.Indicators[name] = 100m;
                        }
                        else if (c.Close < prev2.Low)
                        {
                            enriched.Indicators[name] = -100m;
                        }
                    }
                }

                prev2 = prev1;
                prev1 = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects Identical Three Crows (3 bearish candles opening near the prior close).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> IdenticalThreeCrows(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev2 = null;
        Candle? prev1 = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev2 != null && prev1 != null && prev2.Close < prev2.Open && prev1.Close < prev1.Open && c.Close < c.Open)
                {
                    var tolerance1 = Math.Max(Math.Abs(prev2.Close) * 0.01m, (prev2.High - prev2.Low) * 0.01m);
                    var tolerance2 = Math.Max(Math.Abs(prev1.Close) * 0.01m, (prev1.High - prev1.Low) * 0.01m);

                    if (Math.Abs(prev1.Open - prev2.Close) <= tolerance1 && Math.Abs(c.Open - prev1.Close) <= tolerance2 &&
                        prev1.Close < prev2.Close && c.Close < prev1.Close)
                    {
                        enriched.Indicators[name] = -100m;
                    }
                }

                prev2 = prev1;
                prev1 = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Kicking By Length pattern (direction follows the longer Marubozu).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> KickingByLength(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev != null)
                {
                    var prevBody = Math.Abs(prev.Close - prev.Open);
                    var prevRange = prev.High - prev.Low;
                    var curBody = Math.Abs(c.Close - c.Open);
                    var curRange = c.High - c.Low;
                    var prevMarubozu = prevRange > 0m && prevBody >= (prevRange * 0.95m);
                    var curMarubozu = curRange > 0m && curBody >= (curRange * 0.95m);

                    if (prevMarubozu && curMarubozu)
                    {
                        if (prev.Close < prev.Open && c.Close > c.Open && c.Low > prev.High)
                        {
                            enriched.Indicators[name] = curBody >= prevBody ? 100m : -100m;
                        }
                        else if (prev.Close > prev.Open && c.Close < c.Open && c.High < prev.Low)
                        {
                            enriched.Indicators[name] = curBody >= prevBody ? -100m : 100m;
                        }
                    }
                }

                prev = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Ladder Bottom pattern (5-bar bullish reversal).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> LadderBottom(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev4 = null;
        Candle? prev3 = null;
        Candle? prev2 = null;
        Candle? prev1 = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev4 != null && prev3 != null && prev2 != null && prev1 != null)
                {
                    var fourthUpper = prev1.High - Math.Max(prev1.Open, prev1.Close);
                    var fourthRange = prev1.High - prev1.Low;
                    var fourthBodyHigh = Math.Max(prev1.Open, prev1.Close);

                    if (prev4.Close < prev4.Open && prev3.Close < prev3.Open && prev2.Close < prev2.Open && prev1.Close < prev1.Open &&
                        prev3.Close < prev4.Close && prev2.Close < prev3.Close && prev1.Close < prev2.Close &&
                        fourthRange > 0m && fourthUpper >= (fourthRange * 0.2m) &&
                        c.Close > c.Open && c.Open > fourthBodyHigh && c.Close > prev1.High)
                    {
                        enriched.Indicators[name] = 100m;
                    }
                }

                prev4 = prev3;
                prev3 = prev2;
                prev2 = prev1;
                prev1 = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Mat Hold pattern (5-bar bullish continuation).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> MatHold(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev4 = null;
        Candle? prev3 = null;
        Candle? prev2 = null;
        Candle? prev1 = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev4 != null && prev3 != null && prev2 != null && prev1 != null)
                {
                    var firstBody = Math.Abs(prev4.Close - prev4.Open);
                    var firstRange = prev4.High - prev4.Low;
                    var secondBody = Math.Abs(prev3.Close - prev3.Open);
                    var secondRange = prev3.High - prev3.Low;
                    var thirdBody = Math.Abs(prev2.Close - prev2.Open);
                    var thirdRange = prev2.High - prev2.Low;
                    var fourthBody = Math.Abs(prev1.Close - prev1.Open);
                    var fourthRange = prev1.High - prev1.Low;
                    var finalBody = Math.Abs(c.Close - c.Open);
                    var finalRange = c.High - c.Low;
                    var midpoint = (prev4.Open + prev4.Close) / 2m;
                    var smallPullback = secondRange > 0m && secondBody <= (secondRange * 0.5m) &&
                        thirdRange > 0m && thirdBody <= (thirdRange * 0.5m) &&
                        fourthRange > 0m && fourthBody <= (fourthRange * 0.5m);
                    var bearishCount = (prev3.Close < prev3.Open ? 1 : 0) + (prev2.Close < prev2.Open ? 1 : 0) + (prev1.Close < prev1.Open ? 1 : 0);
                    var maxHigh = Math.Max(Math.Max(prev4.High, prev3.High), Math.Max(prev2.High, prev1.High));

                    if (firstRange > 0m && finalRange > 0m && firstBody >= (firstRange * 0.5m) && finalBody >= (finalRange * 0.5m) &&
                        prev4.Close > prev4.Open && prev3.Low > prev4.High &&
                        prev3.Low > midpoint && prev2.Low > midpoint && prev1.Low > midpoint &&
                        smallPullback && bearishCount >= 2 &&
                        c.Close > c.Open && c.Close > maxHigh)
                    {
                        enriched.Indicators[name] = 100m;
                    }
                }

                prev4 = prev3;
                prev3 = prev2;
                prev2 = prev1;
                prev1 = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects Rising/Falling Three Methods (5-bar continuation).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> RisingFallingThreeMethods(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev4 = null;
        Candle? prev3 = null;
        Candle? prev2 = null;
        Candle? prev1 = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev4 != null && prev3 != null && prev2 != null && prev1 != null)
                {
                    var firstBody = Math.Abs(prev4.Close - prev4.Open);
                    var firstRange = prev4.High - prev4.Low;
                    var secondBody = Math.Abs(prev3.Close - prev3.Open);
                    var thirdBody = Math.Abs(prev2.Close - prev2.Open);
                    var fourthBody = Math.Abs(prev1.Close - prev1.Open);
                    var finalBody = Math.Abs(c.Close - c.Open);
                    var finalRange = c.High - c.Low;
                    var smallBodies = secondBody < firstBody && thirdBody < firstBody && fourthBody < firstBody;
                    var insideFirstRange = prev3.High <= prev4.High && prev3.Low >= prev4.Low &&
                        prev2.High <= prev4.High && prev2.Low >= prev4.Low &&
                        prev1.High <= prev4.High && prev1.Low >= prev4.Low;

                    if (firstRange > 0m && finalRange > 0m && firstBody >= (firstRange * 0.5m) && finalBody >= (finalRange * 0.5m) && smallBodies && insideFirstRange)
                    {
                        if (prev4.Close > prev4.Open && prev3.Close < prev3.Open && prev2.Close < prev2.Open && prev1.Close < prev1.Open &&
                            c.Close > c.Open && c.Close > prev4.High)
                        {
                            enriched.Indicators[name] = 100m;
                        }
                        else if (prev4.Close < prev4.Open && prev3.Close > prev3.Open && prev2.Close > prev2.Open && prev1.Close > prev1.Open &&
                            c.Close < c.Open && c.Close < prev4.Low)
                        {
                            enriched.Indicators[name] = -100m;
                        }
                    }
                }

                prev4 = prev3;
                prev3 = prev2;
                prev2 = prev1;
                prev1 = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Stalled Pattern (3-bar bearish exhaustion after rising candles).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> StalledPattern(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev2 = null;
        Candle? prev1 = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev2 != null && prev1 != null)
                {
                    var prev1Body = Math.Abs(prev1.Close - prev1.Open);
                    var currentBody = Math.Abs(c.Close - c.Open);
                    var currentRange = c.High - c.Low;
                    var currentUpper = c.High - Math.Max(c.Open, c.Close);
                    var openTolerance = (prev1.High - prev1.Low) * 0.1m;

                    if (prev2.Close > prev2.Open && prev1.Close > prev1.Open && c.Close > c.Open &&
                        prev1.Close > prev2.Close && c.Close > prev1.Close &&
                        currentRange > 0m && currentBody < prev1Body && currentUpper <= (currentRange * 0.1m) &&
                        c.Open >= prev1.Open - openTolerance && c.Open <= prev1.Close + openTolerance)
                    {
                        enriched.Indicators[name] = -100m;
                    }
                }

                prev2 = prev1;
                prev1 = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Takuri pattern (Doji-like candle with a very long lower shadow).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Takuri(
        string name,
        CancellationToken cancellationToken = default)
    {
        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;
                var body = Math.Abs(c.Close - c.Open);
                var range = c.High - c.Low;
                var upperShadow = c.High - Math.Max(c.Open, c.Close);
                var lowerShadow = Math.Min(c.Open, c.Close) - c.Low;

                if (range > 0m && body <= (range * 0.05m) && upperShadow <= (range * 0.05m) && lowerShadow >= ((range - lowerShadow) * 3m))
                {
                    enriched.Indicators[name] = 100m;
                }

                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Tasuki Gap pattern (3-bar continuation after a gap).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> TasukiGap(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev2 = null;
        Candle? prev1 = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev2 != null && prev1 != null)
                {
                    if (prev2.Close > prev2.Open && prev1.Close > prev1.Open && prev1.Low > prev2.High &&
                        c.Close < c.Open && c.Open > prev1.Open && c.Open < prev1.Close &&
                        c.Close < prev1.Low && c.Close > prev2.High)
                    {
                        enriched.Indicators[name] = 100m;
                    }
                    else if (prev2.Close < prev2.Open && prev1.Close < prev1.Open && prev1.High < prev2.Low &&
                        c.Close > c.Open && c.Open < prev1.Open && c.Open > prev1.Close &&
                        c.Close > prev1.High && c.Close < prev2.Low)
                    {
                        enriched.Indicators[name] = -100m;
                    }
                }

                prev2 = prev1;
                prev1 = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects a Unique 3 River pattern (3-bar bullish reversal).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Unique3River(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev2 = null;
        Candle? prev1 = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev2 != null && prev1 != null)
                {
                    var firstBody = Math.Abs(prev2.Close - prev2.Open);
                    var firstRange = prev2.High - prev2.Low;
                    var secondBodyHigh = Math.Max(prev1.Open, prev1.Close);
                    var secondBodyLow = Math.Min(prev1.Open, prev1.Close);
                    var firstBodyHigh = Math.Max(prev2.Open, prev2.Close);
                    var firstBodyLow = Math.Min(prev2.Open, prev2.Close);
                    var currentBody = Math.Abs(c.Close - c.Open);
                    var currentRange = c.High - c.Low;

                    if (firstRange > 0m && firstBody >= (firstRange * 0.5m) &&
                        prev2.Close < prev2.Open && prev1.Close < prev1.Open &&
                        secondBodyHigh <= firstBodyHigh && secondBodyLow >= firstBodyLow && prev1.Low < prev2.Low &&
                        currentRange > 0m && currentBody <= (currentRange * 0.4m) && c.Close > c.Open && c.Close < prev1.Close)
                    {
                        enriched.Indicators[name] = 100m;
                    }
                }

                prev2 = prev1;
                prev1 = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects an Upside Gap Two Crows pattern (3-bar bearish reversal).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> UpsideGap2Crows(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev2 = null;
        Candle? prev1 = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev2 != null && prev1 != null)
                {
                    var firstBody = Math.Abs(prev2.Close - prev2.Open);
                    var firstRange = prev2.High - prev2.Low;

                    if (firstRange > 0m && firstBody >= (firstRange * 0.5m) &&
                        prev2.Close > prev2.Open && prev1.Close < prev1.Open && c.Close < c.Open &&
                        prev1.Open > prev2.Close && c.Open > prev1.Open && c.Close < prev1.Close && c.Close > prev2.Close)
                    {
                        enriched.Indicators[name] = -100m;
                    }
                }

                prev2 = prev1;
                prev1 = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>Detects an Upside/Downside Gap Three Methods pattern (3-bar continuation).</summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> XSideGap3Methods(
        string name,
        CancellationToken cancellationToken = default)
    {
        Candle? prev2 = null;
        Candle? prev1 = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prev2 != null && prev1 != null)
                {
                    if (prev2.Close > prev2.Open && prev1.Close > prev1.Open && prev1.Low > prev2.High &&
                        c.Close < c.Open && c.Open > prev1.Open && c.Open < prev1.Close &&
                        c.Close > prev2.Open && c.Close < prev2.Close)
                    {
                        enriched.Indicators[name] = 100m;
                    }
                    else if (prev2.Close < prev2.Open && prev1.Close < prev1.Open && prev1.High < prev2.Low &&
                        c.Close > c.Open && c.Open < prev1.Open && c.Open > prev1.Close &&
                        c.Close < prev2.Open && c.Close > prev2.Close)
                    {
                        enriched.Indicators[name] = -100m;
                    }
                }

                prev2 = prev1;
                prev1 = c;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }
}
