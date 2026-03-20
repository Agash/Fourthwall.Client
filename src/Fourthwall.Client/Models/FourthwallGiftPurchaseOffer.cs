using System.Text.Json.Serialization;

namespace Fourthwall.Client.Models;

/// <summary>
/// The product (offer) being gifted in a gift purchase event.
/// </summary>
public sealed record FourthwallGiftPurchaseOffer
{
    /// <summary>Gets the offer identifier.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>Gets the offer display name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>Gets the offer URL slug.</summary>
    [JsonPropertyName("slug")]
    public string? Slug { get; init; }
}
