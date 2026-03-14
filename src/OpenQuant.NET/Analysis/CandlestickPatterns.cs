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
}
