using System.Text.Json.Serialization;

namespace OpenQuant.Providers.YahooFinance.Dto;

internal sealed record YahooChartResponse
{
    [JsonPropertyName("chart")]
    public YahooChart Chart { get; init; } = default!;
}
