using System.Text.Json.Serialization;

namespace Fourthwall.Client.Models;

/// <summary>
/// A single product line item within an order.
/// </summary>
public sealed record FourthwallOrderLineItem
{
    /// <summary>Gets the product (offer) identifier.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>Gets the product display name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>Gets the product URL slug.</summary>
    [JsonPropertyName("slug")]
    public string? Slug { get; init; }

    /// <summary>Gets the selected variant for this line item.</summary>
    [JsonPropertyName("variant")]
    public FourthwallOrderVariant? Variant { get; init; }
}
