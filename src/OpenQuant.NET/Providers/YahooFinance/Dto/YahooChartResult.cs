using System.Text.Json.Serialization;

namespace OpenQuant.Providers.YahooFinance.Dto;

internal sealed record YahooChartResult
{
    [JsonPropertyName("timestamp")]
    public List<long>? Timestamp { get; init; }

    [JsonPropertyName("indicators")]
    public YahooIndicators Indicators { get; init; } = default!;
}
