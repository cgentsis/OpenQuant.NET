using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// A factory delegate that creates an <see cref="ActionBlock{Candle}"/> indicator
/// targeting the specified output block. The period or any other configuration
/// is expected to be captured by the delegate.
/// </summary>
/// <param name="target">The target block that receives computed indicator values.</param>
/// <param name="cancellationToken">Cancellation token forwarded to the indicator block.</param>
/// <returns>An <see cref="ActionBlock{Candle}"/> that computes the indicator.</returns>
public delegate ActionBlock<Candle> IndicatorFactory(
    ITargetBlock<(DateTimeOffset Timestamp, decimal Value)> target,
    CancellationToken cancellationToken);
