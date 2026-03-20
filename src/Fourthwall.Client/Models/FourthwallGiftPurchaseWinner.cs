using System.Text.Json.Serialization;

namespace Fourthwall.Client.Models;

/// <summary>
/// Represents the winner of an individual gift within a <see cref="FourthwallGiftPurchaseGift"/>.
/// </summary>
public sealed record FourthwallGiftPurchaseWinner
{
    /// <summary>Gets the winner's email address.</summary>
    [JsonPropertyName("email")]
    public string? Email { get; init; }

    /// <summary>Gets the winner's display name / username.</summary>
    [JsonPropertyName("username")]
    public string? Username { get; init; }

    /// <summary>Gets the UTC timestamp when the winner was selected.</summary>
    [JsonPropertyName("selectedAt")]
    public DateTimeOffset? SelectedAt { get; init; }
}
