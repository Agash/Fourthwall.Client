using System.Text.Json.Serialization;

namespace Fourthwall.Client.Models;

/// <summary>
/// Represents an individual gift code within a <see cref="FourthwallGiftPurchaseData"/>.
/// The <c>status</c> discriminator identifies the lifecycle state of the gift.
/// </summary>
/// <remarks>
/// Known <c>status</c> values:
/// <list type="bullet">
///   <item><description><c>AVAILABLE</c> — gift code has been issued and not yet redeemed.</description></item>
///   <item><description><c>CANCELLED</c> — gift code was cancelled.</description></item>
///   <item><description><c>CHANGED_TO_PROMOTION</c> — gift was converted to a promotional code; see <see cref="PromotionId"/>.</description></item>
///   <item><description><c>REDEEMED</c> — gift was redeemed; see <see cref="OrderId"/> and <see cref="OrderFriendlyId"/>.</description></item>
/// </list>
/// </remarks>
public sealed record FourthwallGiftPurchaseGift
{
    /// <summary>
    /// Gets the gift status discriminator.
    /// Known values: <c>AVAILABLE</c>, <c>CANCELLED</c>, <c>CHANGED_TO_PROMOTION</c>, <c>REDEEMED</c>.
    /// </summary>
    [JsonPropertyName("status")]
    public required string Status { get; init; }

    /// <summary>Gets the gift code identifier.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    /// Gets the order identifier created when this gift was redeemed.
    /// Only present when <see cref="Status"/> is <c>REDEEMED</c>.
    /// </summary>
    [JsonPropertyName("orderId")]
    public string? OrderId { get; init; }

    /// <summary>
    /// Gets the human-readable order identifier for the redemption.
    /// Only present when <see cref="Status"/> is <c>REDEEMED</c>.
    /// </summary>
    [JsonPropertyName("orderFriendlyId")]
    public string? OrderFriendlyId { get; init; }

    /// <summary>
    /// Gets the promotion identifier this gift was converted to.
    /// Only present when <see cref="Status"/> is <c>CHANGED_TO_PROMOTION</c>.
    /// </summary>
    [JsonPropertyName("promotionId")]
    public string? PromotionId { get; init; }

    /// <summary>
    /// Gets the winner of this gift.
    /// Present for <c>CHANGED_TO_PROMOTION</c> and <c>REDEEMED</c>; optional for <c>AVAILABLE</c> and <c>CANCELLED</c>.
    /// </summary>
    [JsonPropertyName("winner")]
    public FourthwallGiftPurchaseWinner? Winner { get; init; }
}
