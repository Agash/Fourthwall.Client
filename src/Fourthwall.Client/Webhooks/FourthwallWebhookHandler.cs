using Agash.Webhook.Abstractions;
using Fourthwall.Client.Abstractions;
using Fourthwall.Client.Events;
using Fourthwall.Client.Json;
using Fourthwall.Client.Models;
using Fourthwall.Client.Options;
using Fourthwall.Client.Generated.Models.Openapi.Model.DonationV1;
using Fourthwall.Client.Generated.Models.Openapi.Model.GiftPurchaseV1;
using Fourthwall.Client.Generated.Models.Openapi.Model.MembershipSupporterV1;
using Fourthwall.Client.Generated.Models.Openapi.Model.OrderV1;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Serialization.Json;
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
    public async Task<WebhookHandleResult<FourthwallWebhookEvent>> HandleAsync(
        WebhookRequest request,
        FourthwallWebhookOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(options);

        cancellationToken.ThrowIfCancellationRequested();

        if (!string.Equals(request.Method, "POST", StringComparison.OrdinalIgnoreCase))
        {
            return CreateFailureResult(405, false, false, "Unsupported HTTP method. Fourthwall webhooks must use POST.");
        }

        if (!request.HasContentType("application/json"))
        {
            return CreateFailureResult(400, false, false, "Unsupported content type. Expected application/json.");
        }

        if (!string.IsNullOrEmpty(options.SigningSecret))
        {
            string signatureHeaderName = FourthwallWebhookSignatureVerifier.GetHeaderName(options.SignatureMode);
            string? providedSignature = request.GetFirstHeaderValue(signatureHeaderName);

            if (!_signatureVerifier.Verify(request.Body, providedSignature, options.SigningSecret))
            {
                return CreateFailureResult(401, false, false, $"The Fourthwall webhook signature in header '{signatureHeaderName}' was missing or invalid.");
            }
        }

        FourthwallWebhookEnvelope? envelope;
        try
        {
            envelope = JsonSerializer.Deserialize(request.Body, FourthwallJsonSerializerContext.Default.FourthwallWebhookEnvelope);
        }
        catch (JsonException ex)
        {
            return CreateFailureResult(400, true, false, $"The request body did not contain valid Fourthwall JSON: {ex.Message}");
        }

        if (envelope is null)
        {
            return CreateFailureResult(400, true, false, "The Fourthwall webhook payload could not be deserialized.");
        }

        FourthwallWebhookEvent? evt = await TryMapEnvelopeAsync(envelope, cancellationToken).ConfigureAwait(false);
        if (evt is null)
        {
            return CreateFailureResult(400, true, false, $"The Fourthwall webhook data payload for event type '{envelope.Type}' could not be deserialized.");
        }

        bool isKnownEvent = evt is not FourthwallUnknownWebhookEvent;

        return new WebhookHandleResult<FourthwallWebhookEvent>
        {
            Response = WebhookResponse.Empty(200),
            IsAuthenticated = true,
            IsKnownEvent = isKnownEvent,
            Event = evt,
            FailureReason = null,
        };
    }

    private static async Task<FourthwallWebhookEvent?> TryMapEnvelopeAsync(FourthwallWebhookEnvelope envelope, CancellationToken cancellationToken)
    {
        string rawData = envelope.Data.GetRawText();

        switch (envelope.Type)
        {
            case FourthwallWebhookEventType.OrderPlaced:
            {
                OrderV1? data = await TryDeserializeAsync(rawData, OrderV1.CreateFromDiscriminatorValue, cancellationToken).ConfigureAwait(false);
                return data is null ? null : new FourthwallOrderPlacedWebhookEvent
                {
                    Id = envelope.Id, WebhookId = envelope.WebhookId, ShopId = envelope.ShopId,
                    Type = envelope.Type, ApiVersion = envelope.ApiVersion,
                    CreatedAt = envelope.CreatedAt, TestMode = envelope.TestMode, Data = data,
                };
            }

            case FourthwallWebhookEventType.OrderUpdated:
            {
                OrderUpdatedV1? data = await TryDeserializeAsync(rawData, OrderUpdatedV1.CreateFromDiscriminatorValue, cancellationToken).ConfigureAwait(false);
                return data is null ? null : new FourthwallOrderUpdatedWebhookEvent
                {
                    Id = envelope.Id, WebhookId = envelope.WebhookId, ShopId = envelope.ShopId,
                    Type = envelope.Type, ApiVersion = envelope.ApiVersion,
                    CreatedAt = envelope.CreatedAt, TestMode = envelope.TestMode, Data = data,
                };
            }

            case FourthwallWebhookEventType.Donation:
            {
                DonationV1? data = await TryDeserializeAsync(rawData, DonationV1.CreateFromDiscriminatorValue, cancellationToken).ConfigureAwait(false);
                return data is null ? null : new FourthwallDonationWebhookEvent
                {
                    Id = envelope.Id, WebhookId = envelope.WebhookId, ShopId = envelope.ShopId,
                    Type = envelope.Type, ApiVersion = envelope.ApiVersion,
                    CreatedAt = envelope.CreatedAt, TestMode = envelope.TestMode, Data = data,
                };
            }

            case FourthwallWebhookEventType.SubscriptionPurchased:
            {
                MembershipSupporterV1? data = await TryDeserializeAsync(rawData, MembershipSupporterV1.CreateFromDiscriminatorValue, cancellationToken).ConfigureAwait(false);
                return data is null ? null : new FourthwallSubscriptionPurchasedWebhookEvent
                {
                    Id = envelope.Id, WebhookId = envelope.WebhookId, ShopId = envelope.ShopId,
                    Type = envelope.Type, ApiVersion = envelope.ApiVersion,
                    CreatedAt = envelope.CreatedAt, TestMode = envelope.TestMode, Data = data,
                };
            }

            case FourthwallWebhookEventType.SubscriptionExpired:
            {
                MembershipSupporterV1? data = await TryDeserializeAsync(rawData, MembershipSupporterV1.CreateFromDiscriminatorValue, cancellationToken).ConfigureAwait(false);
                return data is null ? null : new FourthwallSubscriptionExpiredWebhookEvent
                {
                    Id = envelope.Id, WebhookId = envelope.WebhookId, ShopId = envelope.ShopId,
                    Type = envelope.Type, ApiVersion = envelope.ApiVersion,
                    CreatedAt = envelope.CreatedAt, TestMode = envelope.TestMode, Data = data,
                };
            }

            case FourthwallWebhookEventType.SubscriptionChanged:
            {
                MembershipSupporterV1? data = await TryDeserializeAsync(rawData, MembershipSupporterV1.CreateFromDiscriminatorValue, cancellationToken).ConfigureAwait(false);
                return data is null ? null : new FourthwallSubscriptionChangedWebhookEvent
                {
                    Id = envelope.Id, WebhookId = envelope.WebhookId, ShopId = envelope.ShopId,
                    Type = envelope.Type, ApiVersion = envelope.ApiVersion,
                    CreatedAt = envelope.CreatedAt, TestMode = envelope.TestMode, Data = data,
                };
            }

            case FourthwallWebhookEventType.GiftPurchase:
            {
                GiftPurchaseV1? data = await TryDeserializeAsync(rawData, GiftPurchaseV1.CreateFromDiscriminatorValue, cancellationToken).ConfigureAwait(false);
                return data is null ? null : new FourthwallGiftPurchaseWebhookEvent
                {
                    Id = envelope.Id, WebhookId = envelope.WebhookId, ShopId = envelope.ShopId,
                    Type = envelope.Type, ApiVersion = envelope.ApiVersion,
                    CreatedAt = envelope.CreatedAt, TestMode = envelope.TestMode, Data = data,
                };
            }

            default:
                return new FourthwallUnknownWebhookEvent
                {
                    Id = envelope.Id,
                    WebhookId = envelope.WebhookId,
                    ShopId = envelope.ShopId,
                    Type = envelope.Type,
                    ApiVersion = envelope.ApiVersion,
                    CreatedAt = envelope.CreatedAt,
                    TestMode = envelope.TestMode,
                    Data = envelope.Data.Clone(),
                };
        }
    }

    private static async Task<T?> TryDeserializeAsync<T>(string rawJson, ParsableFactory<T> factory, CancellationToken cancellationToken)
        where T : class, IParsable
    {
        try
        {
            return await KiotaJsonSerializer.DeserializeAsync(rawJson, factory, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception)
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
