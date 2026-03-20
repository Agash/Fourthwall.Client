using System.Text.Json;
using System.Text.Json.Serialization;
using Fourthwall.Client.Models;

namespace Fourthwall.Client.Json;

[JsonSerializable(typeof(FourthwallWebhookEnvelope))]
[JsonSerializable(typeof(FourthwallMoney))]
[JsonSerializable(typeof(FourthwallAddress))]
[JsonSerializable(typeof(FourthwallDonationData))]
[JsonSerializable(typeof(FourthwallDonationAmounts))]
[JsonSerializable(typeof(FourthwallOrderData))]
[JsonSerializable(typeof(FourthwallOrderAmounts))]
[JsonSerializable(typeof(FourthwallOrderShipping))]
[JsonSerializable(typeof(FourthwallOrderLineItem))]
[JsonSerializable(typeof(FourthwallOrderVariant))]
[JsonSerializable(typeof(FourthwallOrderUpdate))]
[JsonSerializable(typeof(FourthwallOrderUpdatedData))]
[JsonSerializable(typeof(FourthwallMembershipSupporterData))]
[JsonSerializable(typeof(FourthwallMembershipSubscriptionData))]
[JsonSerializable(typeof(FourthwallTierVariantData))]
[JsonSerializable(typeof(FourthwallGiftPurchaseData))]
[JsonSerializable(typeof(FourthwallGiftPurchaseAmounts))]
[JsonSerializable(typeof(FourthwallGiftPurchaseOffer))]
[JsonSerializable(typeof(FourthwallGiftPurchaseGift))]
[JsonSerializable(typeof(FourthwallGiftPurchaseWinner))]
[JsonSerializable(typeof(IReadOnlyList<FourthwallOrderLineItem>))]
[JsonSerializable(typeof(IReadOnlyList<FourthwallGiftPurchaseGift>))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.Unspecified,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    WriteIndented = false)]
internal sealed partial class FourthwallJsonSerializerContext : JsonSerializerContext;
