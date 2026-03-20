using Agash.Webhook.Abstractions;
using Fourthwall.Client.Abstractions;
using Fourthwall.Client.Events;
using Fourthwall.Client.Json;
using Fourthwall.Client.Models;
using Fourthwall.Client.Options;
using System.Text.Json;

namespace Fourthwall.Client.Webhooks;

/// <summary>
/// Provides the default transport-neutral implementation for processing Fourthwall webhook requests.
/// </summary>
public sealed class FourthwallWebhookHandler : IFourthwallWebhookHandler
{
    private readonly FourthwallWebhookSignatureVerifier _signatureVerifier;

    /// <summary>
    /// Initializes a new instance of the <see cref="FourthwallWebhookHandler"/> class.
    /// </summary>
    /// <param name="signatureVerifier">The signature verifier used to validate webhook deliveries.</param>
    public FourthwallWebhookHandler(FourthwallWebhookSignatureVerifier signatureVerifier)
    {
        _signatureVerifier = signatureVerifier ?? throw new ArgumentNullException(nameof(signatureVerifier));
    }

    /// <inheritdoc />
    public Task<WebhookHandleResult<FourthwallWebhookEvent>> HandleAsync(
        WebhookRequest request,
        FourthwallWebhookOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrEmpty(options.SigningSecret);

        cancellationToken.ThrowIfCancellationRequested();

        if (!string.Equals(request.Method, "POST", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(CreateFailureResult(405, false, false, "Unsupported HTTP method. Fourthwall webhooks must use POST."));
        }

        if (!request.HasContentType("application/json"))
        {
            return Task.FromResult(CreateFailureResult(400, false, false, "Unsupported content type. Expected application/json."));
        }

        string signatureHeaderName = FourthwallWebhookSignatureVerifier.GetHeaderName(options.SignatureMode);
        string? providedSignature = request.GetFirstHeaderValue(signatureHeaderName);

        if (!_signatureVerifier.Verify(request.Body, providedSignature, options.SigningSecret))
        {
            return Task.FromResult(CreateFailureResult(401, false, false, $"The Fourthwall webhook signature in header '{signatureHeaderName}' was missing or invalid."));
        }

        FourthwallWebhookEnvelope? envelope;
        try
        {
            envelope = JsonSerializer.Deserialize(request.Body, FourthwallJsonSerializerContext.Default.FourthwallWebhookEnvelope);
        }
        catch (JsonException ex)
        {
            return Task.FromResult(CreateFailureResult(400, true, false, $"The request body did not contain valid Fourthwall JSON: {ex.Message}"));
        }

        if (envelope is null)
        {
            return Task.FromResult(CreateFailureResult(400, true, false, "The Fourthwall webhook payload could not be deserialized."));
        }

        FourthwallWebhookEvent? evt = TryMapEnvelope(envelope);
        if (evt is null)
        {
            return Task.FromResult(CreateFailureResult(400, true, false, $"The Fourthwall webhook data payload for event type '{envelope.Type}' could not be deserialized."));
        }

        bool isKnownEvent = evt is not FourthwallUnknownWebhookEvent;

        return Task.FromResult(new WebhookHandleResult<FourthwallWebhookEvent>
        {
            Response = WebhookResponse.Empty(200),
            IsAuthenticated = true,
            IsKnownEvent = isKnownEvent,
            Event = evt,
            FailureReason = null,
        });
    }

    private static FourthwallWebhookEvent? TryMapEnvelope(FourthwallWebhookEnvelope envelope)
    {
        JsonSerializerOptions options = FourthwallJsonSerializerContext.Default.Options;

        return envelope.Type switch
        {
            FourthwallWebhookEventType.OrderPlaced => TryDeserialize<FourthwallOrderData>(envelope.Data, options) is { } orderData
                ? new FourthwallOrderPlacedWebhookEvent
                {
                    Id = envelope.Id,
                    WebhookId = envelope.WebhookId,
                    ShopId = envelope.ShopId,
                    Type = envelope.Type,
                    ApiVersion = envelope.ApiVersion,
                    CreatedAt = envelope.CreatedAt,
                    TestMode = envelope.TestMode,
                    Data = orderData,
                }
                : null,

            FourthwallWebhookEventType.OrderUpdated => TryDeserialize<FourthwallOrderUpdatedData>(envelope.Data, options) is { } orderUpdatedData
                ? new FourthwallOrderUpdatedWebhookEvent
                {
                    Id = envelope.Id,
                    WebhookId = envelope.WebhookId,
                    ShopId = envelope.ShopId,
                    Type = envelope.Type,
                    ApiVersion = envelope.ApiVersion,
                    CreatedAt = envelope.CreatedAt,
                    TestMode = envelope.TestMode,
                    Data = orderUpdatedData,
                }
                : null,

            FourthwallWebhookEventType.Donation => TryDeserialize<FourthwallDonationData>(envelope.Data, options) is { } donationData
                ? new FourthwallDonationWebhookEvent
                {
                    Id = envelope.Id,
                    WebhookId = envelope.WebhookId,
                    ShopId = envelope.ShopId,
                    Type = envelope.Type,
                    ApiVersion = envelope.ApiVersion,
                    CreatedAt = envelope.CreatedAt,
                    TestMode = envelope.TestMode,
                    Data = donationData,
                }
                : null,

            FourthwallWebhookEventType.SubscriptionPurchased => TryDeserialize<FourthwallMembershipSupporterData>(envelope.Data, options) is { } subPurchasedData
                ? new FourthwallSubscriptionPurchasedWebhookEvent
                {
                    Id = envelope.Id,
                    WebhookId = envelope.WebhookId,
                    ShopId = envelope.ShopId,
                    Type = envelope.Type,
                    ApiVersion = envelope.ApiVersion,
                    CreatedAt = envelope.CreatedAt,
                    TestMode = envelope.TestMode,
                    Data = subPurchasedData,
                }
                : null,

            FourthwallWebhookEventType.SubscriptionExpired => TryDeserialize<FourthwallMembershipSupporterData>(envelope.Data, options) is { } subExpiredData
                ? new FourthwallSubscriptionExpiredWebhookEvent
                {
                    Id = envelope.Id,
                    WebhookId = envelope.WebhookId,
                    ShopId = envelope.ShopId,
                    Type = envelope.Type,
                    ApiVersion = envelope.ApiVersion,
                    CreatedAt = envelope.CreatedAt,
                    TestMode = envelope.TestMode,
                    Data = subExpiredData,
                }
                : null,

            FourthwallWebhookEventType.SubscriptionChanged => TryDeserialize<FourthwallMembershipSupporterData>(envelope.Data, options) is { } subChangedData
                ? new FourthwallSubscriptionChangedWebhookEvent
                {
                    Id = envelope.Id,
                    WebhookId = envelope.WebhookId,
                    ShopId = envelope.ShopId,
                    Type = envelope.Type,
                    ApiVersion = envelope.ApiVersion,
                    CreatedAt = envelope.CreatedAt,
                    TestMode = envelope.TestMode,
                    Data = subChangedData,
                }
                : null,

            FourthwallWebhookEventType.GiftPurchase => TryDeserialize<FourthwallGiftPurchaseData>(envelope.Data, options) is { } giftData
                ? new FourthwallGiftPurchaseWebhookEvent
                {
                    Id = envelope.Id,
                    WebhookId = envelope.WebhookId,
                    ShopId = envelope.ShopId,
                    Type = envelope.Type,
                    ApiVersion = envelope.ApiVersion,
                    CreatedAt = envelope.CreatedAt,
                    TestMode = envelope.TestMode,
                    Data = giftData,
                }
                : null,

            _ => new FourthwallUnknownWebhookEvent
            {
                Id = envelope.Id,
                WebhookId = envelope.WebhookId,
                ShopId = envelope.ShopId,
                Type = envelope.Type,
                ApiVersion = envelope.ApiVersion,
                CreatedAt = envelope.CreatedAt,
                TestMode = envelope.TestMode,
                Data = envelope.Data.Clone(),
            },
        };
    }

    private static T? TryDeserialize<T>(JsonElement data, JsonSerializerOptions options)
        where T : class
    {
        try
        {
            return data.Deserialize<T>(options);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static WebhookHandleResult<FourthwallWebhookEvent> CreateFailureResult(
        int statusCode,
        bool isAuthenticated,
        bool isKnownEvent,
        string failureReason)
    {
        return new()
        {
            Response = WebhookResponse.Empty(statusCode),
            IsAuthenticated = isAuthenticated,
            IsKnownEvent = isKnownEvent,
            Event = null,
            FailureReason = failureReason,
        };
    }
}
