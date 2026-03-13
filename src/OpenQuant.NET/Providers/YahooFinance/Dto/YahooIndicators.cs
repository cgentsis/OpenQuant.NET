using System.Text.Json.Serialization;

namespace OpenQuant.Providers.YahooFinance.Dto;

internal sealed record YahooIndicators
{
    [JsonPropertyName("quote")]
    public List<YahooQuote>? Quote { get; init; }
}
