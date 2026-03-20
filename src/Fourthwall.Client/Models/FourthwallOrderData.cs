using System.Text.Json.Serialization;

namespace Fourthwall.Client.Models;

/// <summary>
/// Typed payload for the <c>ORDER_PLACED</c> webhook event (<c>OrderV1</c>).
/// </summary>
public sealed record FourthwallOrderData
{
    /// <summary>Gets the order identifier.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>Gets the shop identifier.</summary>
    [JsonPropertyName("shopId")]
    public string? ShopId { get; init; }

    /// <summary>Gets the human-readable friendly order identifier.</summary>
    [JsonPropertyName("friendlyId")]
    public string? FriendlyId { get; init; }

    /// <summary>Gets the order status (CONFIRMED, SHIPPED, DELIVERED, etc.).</summary>
    [JsonPropertyName("status")]
    public string? Status { get; init; }

    /// <summary>Gets the customer's email address.</summary>
    [JsonPropertyName("email")]
    public string? Email { get; init; }

    /// <summary>Gets the customer's display name / username.</summary>
    [JsonPropertyName("username")]
    public string? Username { get; init; }

    /// <summary>Gets the optional message left by the customer.</summary>
    [JsonPropertyName("message")]
    public string? Message { get; init; }

    /// <summary>Gets the monetary amounts breakdown for this order.</summary>
    [JsonPropertyName("amounts")]
    public FourthwallOrderAmounts? Amounts { get; init; }

    /// <summary>Gets the shipping information including address.</summary>
    [JsonPropertyName("shipping")]
    public FourthwallOrderShipping? Shipping { get; init; }

    /// <summary>Gets the line items (products) in the order.</summary>
    [JsonPropertyName("offers")]
    public IReadOnlyList<FourthwallOrderLineItem>? Offers { get; init; }

    /// <summary>Gets the UTC timestamp when the order was created.</summary>
    [JsonPropertyName("createdAt")]
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>Gets the UTC timestamp when the order was last updated.</summary>
    [JsonPropertyName("updatedAt")]
    public DateTimeOffset? UpdatedAt { get; init; }
}
