using System.Text.Json.Serialization;

namespace OpenQuant.Providers.YahooFinance.Dto;

internal sealed record YahooQuote
{
    [JsonPropertyName("open")]
    public List<decimal?>? Open { get; init; }

    [JsonPropertyName("high")]
    public List<decimal?>? High { get; init; }

    [JsonPropertyName("low")]
    public List<decimal?>? Low { get; init; }

    [JsonPropertyName("close")]
    public List<decimal?>? Close { get; init; }

    [JsonPropertyName("volume")]
    public List<long?>? Volume { get; init; }
}
