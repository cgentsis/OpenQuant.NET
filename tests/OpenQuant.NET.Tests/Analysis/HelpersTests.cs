using System;
using OpenQuant.Analysis;
using Xunit;

namespace OpenQuant.Tests.Analysis;

public class HelpersTests
{
    [Fact]
    public void ComputeWMACircular_ShouldCalculateCorrectly()
    {
        // Arrange
        var buffer = new decimal[] { 10m, 20m, 30m, 40m, 50m };
        var startIndex = 0;
        var count = 5;

        // Act
        var result = Helpers.ComputeWMACircular(buffer, startIndex, count);

        // Assert
        // WMA calculation: (1*10 + 2*20 + 3*30 + 4*40 + 5*50) / (1+2+3+4+5)
        // = (10 + 40 + 90 + 160 + 250) / 15 = 550 / 15 = 36.666...
        Assert.Equal(36.666666666666666666666666667m, result, precision: 28);
    }

    [Fact]
    public void ComputeWMA_ShouldCalculateCorrectly()
    {
        // Arrange
        var buffer = new decimal[] { 10m, 20m, 30m, 40m, 50m };
        var period = 5;

        // Act
        var result = Helpers.ComputeWMA(buffer, period);

        // Assert
        // WMA calculation: (1*10 + 2*20 + 3*30 + 4*40 + 5*50) / (1+2+3+4+5)
        // = (10 + 40 + 90 + 160 + 250) / 15 = 550 / 15 = 36.666...
        Assert.Equal(36.666666666666666666666666667m, result, precision: 28);
    }

    [Fact]
    public void ComputeEMA_ShouldCalculateCorrectly()
    {
        // Arrange
        var previousEma = 100m;
        var price = 110m;
        var k = 0.2m; // 2/(10+1) for period 10

        // Act
        var result = Helpers.ComputeEMA(previousEma, price, k);

        // Assert
        // EMA calculation: (110 * 0.2) + (100 * 0.8) = 22 + 80 = 102
        Assert.Equal(102m, result);
    }
}