using System.Text.Json.Serialization;

namespace Fourthwall.Client.Models;

/// <summary>
/// Represents a postal address used in orders and billing.
/// </summary>
public sealed record FourthwallAddress
{
    /// <summary>Gets the recipient full name.</summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>Gets the primary street address line.</summary>
    [JsonPropertyName("address1")]
    public required string Address1 { get; init; }

    /// <summary>Gets the secondary street address line (apartment, suite, etc.).</summary>
    [JsonPropertyName("address2")]
    public string? Address2 { get; init; }

    /// <summary>Gets the city name.</summary>
    [JsonPropertyName("city")]
    public required string City { get; init; }

    /// <summary>Gets the state or province code.</summary>
    [JsonPropertyName("state")]
    public required string State { get; init; }

    /// <summary>Gets the country code.</summary>
    [JsonPropertyName("country")]
    public required string Country { get; init; }

    /// <summary>Gets the postal or zip code.</summary>
    [JsonPropertyName("zip")]
    public string? Zip { get; init; }

    /// <summary>Gets the telephone number.</summary>
    [JsonPropertyName("phone")]
    public string? Phone { get; init; }
}
