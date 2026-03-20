using System.Text.Json.Serialization;

namespace Fourthwall.Client.Models;

/// <summary>
/// Represents a monetary amount with its associated currency code (ISO 4217).
/// Maps to the <c>Money</c> schema in the Fourthwall OpenAPI spec.
/// </summary>
public sealed record FourthwallMoney
{
    /// <summary>Gets the monetary amount value.</summary>
    [JsonPropertyName("value")]
    public required decimal Value { get; init; }

    /// <summary>Gets the ISO 4217 currency code (e.g. "USD").</summary>
    [JsonPropertyName("currency")]
    public required string Currency { get; init; }
}
