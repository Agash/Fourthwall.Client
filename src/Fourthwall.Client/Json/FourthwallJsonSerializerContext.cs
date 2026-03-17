using Fourthwall.Client.Models;
using System.Text.Json.Serialization;

namespace Fourthwall.Client.Json;

/// <summary>
/// Provides source-generated JSON serialization metadata for Fourthwall webhook models.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.Unspecified,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    WriteIndented = false)]
[JsonSerializable(typeof(FourthwallWebhookEnvelope))]
internal sealed partial class FourthwallJsonSerializerContext : JsonSerializerContext;