using System.Text.Json;
using System.Text.Json.Serialization;
using Fourthwall.Client.Models;

namespace Fourthwall.Client.Json;

// Only the envelope needs STJ deserialization. All webhook data payloads (OrderV1,
// DonationV1, GiftPurchaseV1, MembershipSupporterV1, OrderUpdatedV1) are deserialized
// through Kiota's own JsonParseNodeFactory via KiotaJsonSerializer, which reuses the
// same generated IParsable types returned by the REST API client.
[JsonSerializable(typeof(FourthwallWebhookEnvelope))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.Unspecified,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    WriteIndented = false)]
internal sealed partial class FourthwallJsonSerializerContext : JsonSerializerContext;
