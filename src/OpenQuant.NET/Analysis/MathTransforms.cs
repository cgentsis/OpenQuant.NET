using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// Provides mathematical transforms of the closing price as TPL Dataflow
/// <see cref="TransformBlock{TInput,TOutput}"/> stages. Each result is stored in
/// <see cref="EnrichedCandle.Indicators"/> under the supplied name, which defaults to the method name.
/// Inputs outside a function's domain are skipped rather than producing a value.
/// </summary>
public static class MathTransforms
{
    /// <summary>Creates a transform block that computes the arc cosine of the close price.</summary>
    /// <param name="period">The period, accepted for API consistency and validated to be positive.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Acos(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default) =>
        CreateTransform(period, name, cancellationToken, nameof(Acos), close =>
            close is >= -1m and <= 1m ? (decimal)Math.Acos((double)close) : null);

    /// <summary>Returns the number of input bars consumed before <see cref="Acos"/> produces its first value.</summary>
    /// <param name="period">The period, ignored because the transform is stateless.</param>
    /// <returns>Always zero.</returns>
#pragma warning disable S3400
    public static int AcosLookback(int period = 14) => 0;
#pragma warning restore S3400

    /// <summary>Creates a transform block that computes the arc sine of the close price.</summary>
    /// <param name="period">The period, accepted for API consistency and validated to be positive.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Asin(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default) =>
        CreateTransform(period, name, cancellationToken, nameof(Asin), close =>
            close is >= -1m and <= 1m ? (decimal)Math.Asin((double)close) : null);

    /// <summary>Returns the number of input bars consumed before <see cref="Asin"/> produces its first value.</summary>
    /// <param name="period">The period, ignored because the transform is stateless.</param>
    /// <returns>Always zero.</returns>
#pragma warning disable S3400
    public static int AsinLookback(int period = 14) => 0;
#pragma warning restore S3400

    /// <summary>Creates a transform block that computes the arc tangent of the close price.</summary>
    /// <param name="period">The period, accepted for API consistency and validated to be positive.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Atan(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default) =>
        CreateTransform(period, name, cancellationToken, nameof(Atan), close => (decimal)Math.Atan((double)close));

    /// <summary>Returns the number of input bars consumed before <see cref="Atan"/> produces its first value.</summary>
    /// <param name="period">The period, ignored because the transform is stateless.</param>
    /// <returns>Always zero.</returns>
#pragma warning disable S3400
    public static int AtanLookback(int period = 14) => 0;
#pragma warning restore S3400

    /// <summary>Creates a transform block that computes the ceiling of the close price.</summary>
    /// <param name="period">The period, accepted for API consistency and validated to be positive.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Ceil(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default) =>
        CreateTransform(period, name, cancellationToken, nameof(Ceil), close => Math.Ceiling(close));

    /// <summary>Returns the number of input bars consumed before <see cref="Ceil"/> produces its first value.</summary>
    /// <param name="period">The period, ignored because the transform is stateless.</param>
    /// <returns>Always zero.</returns>
#pragma warning disable S3400
    public static int CeilLookback(int period = 14) => 0;
#pragma warning restore S3400

    /// <summary>Creates a transform block that computes the cosine of the close price.</summary>
    /// <param name="period">The period, accepted for API consistency and validated to be positive.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Cos(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default) =>
        CreateTransform(period, name, cancellationToken, nameof(Cos), close => (decimal)Math.Cos((double)close));

    /// <summary>Returns the number of input bars consumed before <see cref="Cos"/> produces its first value.</summary>
    /// <param name="period">The period, ignored because the transform is stateless.</param>
    /// <returns>Always zero.</returns>
#pragma warning disable S3400
    public static int CosLookback(int period = 14) => 0;
#pragma warning restore S3400

    /// <summary>Creates a transform block that computes the hyperbolic cosine of the close price.</summary>
    /// <param name="period">The period, accepted for API consistency and validated to be positive.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Cosh(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default) =>
        CreateTransform(period, name, cancellationToken, nameof(Cosh), close => (decimal)Math.Cosh((double)close));

    /// <summary>Returns the number of input bars consumed before <see cref="Cosh"/> produces its first value.</summary>
    /// <param name="period">The period, ignored because the transform is stateless.</param>
    /// <returns>Always zero.</returns>
#pragma warning disable S3400
    public static int CoshLookback(int period = 14) => 0;
#pragma warning restore S3400

    /// <summary>Creates a transform block that computes the exponential of the close price.</summary>
    /// <param name="period">The period, accepted for API consistency and validated to be positive.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Exp(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default) =>
        CreateTransform(period, name, cancellationToken, nameof(Exp), close =>
        {
            var result = Math.Exp((double)close);
            return double.IsFinite(result) && result <= (double)decimal.MaxValue ? (decimal?)result : null;
        });

    /// <summary>Returns the number of input bars consumed before <see cref="Exp"/> produces its first value.</summary>
    /// <param name="period">The period, ignored because the transform is stateless.</param>
    /// <returns>Always zero.</returns>
#pragma warning disable S3400
    public static int ExpLookback(int period = 14) => 0;
#pragma warning restore S3400

    /// <summary>Creates a transform block that computes the floor of the close price.</summary>
    /// <param name="period">The period, accepted for API consistency and validated to be positive.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Floor(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default) =>
        CreateTransform(period, name, cancellationToken, nameof(Floor), close => Math.Floor(close));

    /// <summary>Returns the number of input bars consumed before <see cref="Floor"/> produces its first value.</summary>
    /// <param name="period">The period, ignored because the transform is stateless.</param>
    /// <returns>Always zero.</returns>
#pragma warning disable S3400
    public static int FloorLookback(int period = 14) => 0;
#pragma warning restore S3400

    /// <summary>Creates a transform block that computes the natural logarithm of the close price.</summary>
    /// <param name="period">The period, accepted for API consistency and validated to be positive.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Ln(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default) =>
        CreateTransform(period, name, cancellationToken, nameof(Ln), close =>
            close > 0m ? (decimal)Math.Log((double)close) : null);

    /// <summary>Returns the number of input bars consumed before <see cref="Ln"/> produces its first value.</summary>
    /// <param name="period">The period, ignored because the transform is stateless.</param>
    /// <returns>Always zero.</returns>
#pragma warning disable S3400
    public static int LnLookback(int period = 14) => 0;
#pragma warning restore S3400

    /// <summary>Creates a transform block that computes the base-10 logarithm of the close price.</summary>
    /// <param name="period">The period, accepted for API consistency and validated to be positive.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Log10(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default) =>
        CreateTransform(period, name, cancellationToken, nameof(Log10), close =>
            close > 0m ? (decimal)Math.Log10((double)close) : null);

    /// <summary>Returns the number of input bars consumed before <see cref="Log10"/> produces its first value.</summary>
    /// <param name="period">The period, ignored because the transform is stateless.</param>
    /// <returns>Always zero.</returns>
#pragma warning disable S3400
    public static int Log10Lookback(int period = 14) => 0;
#pragma warning restore S3400

    /// <summary>Creates a transform block that computes the sine of the close price.</summary>
    /// <param name="period">The period, accepted for API consistency and validated to be positive.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Sin(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default) =>
        CreateTransform(period, name, cancellationToken, nameof(Sin), close => (decimal)Math.Sin((double)close));

    /// <summary>Returns the number of input bars consumed before <see cref="Sin"/> produces its first value.</summary>
    /// <param name="period">The period, ignored because the transform is stateless.</param>
    /// <returns>Always zero.</returns>
#pragma warning disable S3400
    public static int SinLookback(int period = 14) => 0;
#pragma warning restore S3400

    /// <summary>Creates a transform block that computes the hyperbolic sine of the close price.</summary>
    /// <param name="period">The period, accepted for API consistency and validated to be positive.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Sinh(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default) =>
        CreateTransform(period, name, cancellationToken, nameof(Sinh), close => (decimal)Math.Sinh((double)close));

    /// <summary>Returns the number of input bars consumed before <see cref="Sinh"/> produces its first value.</summary>
    /// <param name="period">The period, ignored because the transform is stateless.</param>
    /// <returns>Always zero.</returns>
#pragma warning disable S3400
    public static int SinhLookback(int period = 14) => 0;
#pragma warning restore S3400

    /// <summary>Creates a transform block that computes the square root of the close price.</summary>
    /// <param name="period">The period, accepted for API consistency and validated to be positive.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Sqrt(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default) =>
        CreateTransform(period, name, cancellationToken, nameof(Sqrt), close =>
            close >= 0m ? (decimal)Math.Sqrt((double)close) : null);

    /// <summary>Returns the number of input bars consumed before <see cref="Sqrt"/> produces its first value.</summary>
    /// <param name="period">The period, ignored because the transform is stateless.</param>
    /// <returns>Always zero.</returns>
#pragma warning disable S3400
    public static int SqrtLookback(int period = 14) => 0;
#pragma warning restore S3400

    /// <summary>Creates a transform block that computes the tangent of the close price.</summary>
    /// <param name="period">The period, accepted for API consistency and validated to be positive.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Tan(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default) =>
        CreateTransform(period, name, cancellationToken, nameof(Tan), close => (decimal)Math.Tan((double)close));

    /// <summary>Returns the number of input bars consumed before <see cref="Tan"/> produces its first value.</summary>
    /// <param name="period">The period, ignored because the transform is stateless.</param>
    /// <returns>Always zero.</returns>
#pragma warning disable S3400
    public static int TanLookback(int period = 14) => 0;
#pragma warning restore S3400

    /// <summary>Creates a transform block that computes the hyperbolic tangent of the close price.</summary>
    /// <param name="period">The period, accepted for API consistency and validated to be positive.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Tanh(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default) =>
        CreateTransform(period, name, cancellationToken, nameof(Tanh), close => (decimal)Math.Tanh((double)close));

    /// <summary>Returns the number of input bars consumed before <see cref="Tanh"/> produces its first value.</summary>
    /// <param name="period">The period, ignored because the transform is stateless.</param>
    /// <returns>Always zero.</returns>
#pragma warning disable S3400
    public static int TanhLookback(int period = 14) => 0;
#pragma warning restore S3400

    private static TransformBlock<EnrichedCandle, EnrichedCandle> CreateTransform(
        int period,
        string? name,
        CancellationToken cancellationToken,
        string defaultName,
        Func<decimal, decimal?> compute)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        var key = name ?? defaultName;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var value = compute(enriched.Candle.Close);
                if (value.HasValue)
                {
                    enriched.Indicators[key] = value.Value;
                }

                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }
}
