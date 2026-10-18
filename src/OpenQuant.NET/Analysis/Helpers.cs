using System;
using System.Collections.Generic;

namespace OpenQuant.Analysis;

/// <summary>
/// Provides shared numeric helper methods used by various analysis components.
/// </summary>
internal static class Helpers
{
    /// <summary>
    /// Computes the weighted moving average using a circular buffer approach.
    /// </summary>
    /// <param name="buffer">The buffer of values.</param>
    /// <param name="startIndex">The start index in the buffer.</param>
    /// <param name="count">The number of elements to process.</param>
    /// <returns>The weighted moving average.</returns>
    internal static decimal ComputeWMACircular(decimal[] buffer, int startIndex, int count)
    {
        var weightedSum = 0m;
        var length = buffer.Length;

        for (var i = 0; i < count; i++)
        {
            weightedSum += buffer[(startIndex + i) % length] * (i + 1);
        }

        return weightedSum / (count * (count + 1) / 2m);
    }

    /// <summary>
    /// Computes the exponential moving average using the recurrence relation.
    /// </summary>
    /// <param name="previousEma">The previous EMA value.</param>
    /// <param name="price">The current price.</param>
    /// <param name="k">The smoothing factor.</param>
    /// <returns>The new EMA value.</returns>
    internal static decimal ComputeEMA(decimal previousEma, decimal price, decimal k)
    {
        return (price * k) + (previousEma * (1 - k));
    }

    /// <summary>
    /// Computes the weighted moving average from a buffer of values.
    /// </summary>
    /// <param name="buffer">The buffer of values.</param>
    /// <param name="period">The period for the WMA calculation.</param>
    /// <returns>The weighted moving average.</returns>
    internal static decimal ComputeWMA(decimal[] buffer, int period)
    {
        var weightedSum = 0m;
        var divisor = period * (period + 1) / 2m;

        for (var i = 0; i < period; i++)
        {
            weightedSum += buffer[i] * (i + 1);
        }

        return weightedSum / divisor;
    }
}