using System.Text.Json.Serialization;

namespace Fourthwall.Client.Models;

/// <summary>
/// Typed payload for the <c>GIFT_PURCHASE</c> webhook event (<c>GiftPurchaseV1</c>).
/// </summary>
public sealed record FourthwallGiftPurchaseData
{
    /// <summary>Gets the gift purchase identifier.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>Gets the human-readable friendly identifier.</summary>
    [JsonPropertyName("friendlyId")]
    public string? FriendlyId { get; init; }

    /// <summary>Gets the shop identifier.</summary>
    [JsonPropertyName("shopId")]
    public string? ShopId { get; init; }

    /// <summary>Gets the purchaser's email address.</summary>
    [JsonPropertyName("email")]
    public required string Email { get; init; }

    /// <summary>Gets the purchaser's display name / username.</summary>
    [JsonPropertyName("username")]
    public string? Username { get; init; }

    /// <summary>Gets the optional message from the purchaser.</summary>
    [JsonPropertyName("message")]
    public string? Message { get; init; }

    /// <summary>Gets the monetary amounts for this gift purchase.</summary>
    [JsonPropertyName("amounts")]
    public required FourthwallGiftPurchaseAmounts Amounts { get; init; }

    /// <summary>Gets the number of gift codes purchased.</summary>
    [JsonPropertyName("quantity")]
    public int Quantity { get; init; }

    /// <summary>Gets the offer (product) being gifted.</summary>
    [JsonPropertyName("offer")]
    public FourthwallGiftPurchaseOffer? Offer { get; init; }

    /// <summary>
    /// Gets the individual gift codes generated for this purchase.
    /// Each entry carries its current lifecycle status via <see cref="FourthwallGiftPurchaseGift.Status"/>.
    /// </summary>
    [JsonPropertyName("gifts")]
    public IReadOnlyList<FourthwallGiftPurchaseGift>? Gifts { get; init; }

    /// <summary>Gets the UTC timestamp when the gift purchase was created.</summary>
    [JsonPropertyName("createdAt")]
    public DateTimeOffset? CreatedAt { get; init; }
}
