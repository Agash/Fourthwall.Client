using System.Text.Json.Serialization;

namespace Fourthwall.Client.Models;

/// <summary>
/// A specific product variant selected within an order line item, including quantity ordered.
/// </summary>
public sealed record FourthwallOrderVariant
{
    /// <summary>Gets the variant identifier.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>Gets the variant display name (e.g. "Black, L").</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>Gets the SKU code for this variant.</summary>
    [JsonPropertyName("sku")]
    public string? Sku { get; init; }

    /// <summary>Gets the quantity ordered for this variant.</summary>
    [JsonPropertyName("quantity")]
    public int? Quantity { get; init; }

    /// <summary>Gets the unit price for this variant.</summary>
    [JsonPropertyName("unitPrice")]
    public FourthwallMoney? UnitPrice { get; init; }
}
