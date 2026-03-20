using System.Text.Json.Serialization;

namespace Fourthwall.Client.Models;

/// <summary>
/// Typed payload for the <c>DONATION</c> webhook event (<c>DonationV1</c>).
/// </summary>
public sealed record FourthwallDonationData
{
    /// <summary>Gets the donation identifier.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>Gets the shop identifier.</summary>
    [JsonPropertyName("shopId")]
    public string? ShopId { get; init; }

    /// <summary>Gets the donation status (OPEN, COMPLETED, ABANDONED, CANCELLED, FAILED).</summary>
    [JsonPropertyName("status")]
    public string? Status { get; init; }

    /// <summary>Gets the supporter's email address.</summary>
    [JsonPropertyName("email")]
    public required string Email { get; init; }

    /// <summary>Gets the supporter's display name / username.</summary>
    [JsonPropertyName("username")]
    public string? Username { get; init; }

    /// <summary>Gets the optional message left by the supporter.</summary>
    [JsonPropertyName("message")]
    public string? Message { get; init; }

    /// <summary>Gets the monetary amounts for this donation.</summary>
    [JsonPropertyName("amounts")]
    public required FourthwallDonationAmounts Amounts { get; init; }

    /// <summary>Gets the UTC timestamp when the donation was created.</summary>
    [JsonPropertyName("createdAt")]
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>Gets the UTC timestamp when the donation was last updated.</summary>
    [JsonPropertyName("updatedAt")]
    public DateTimeOffset? UpdatedAt { get; init; }
}
