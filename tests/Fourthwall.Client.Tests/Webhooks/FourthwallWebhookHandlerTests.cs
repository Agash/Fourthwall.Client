using Agash.Webhook.Abstractions;
using FluentAssertions;
using Fourthwall.Client.Events;
using Fourthwall.Client.Options;
using Fourthwall.Client.Webhooks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography;
using System.Text;

namespace Fourthwall.Client.Tests.Webhooks;

[TestClass]
public sealed class FourthwallWebhookHandlerTests
{
    private const string TestSigningSecret = "test-signing-secret";

    // Minimal valid envelope that deserializes to FourthwallUnknownWebhookEvent
    // (uses an unrecognised type so we don't need a full concrete data model).
    private const string MinimalEnvelopeJson = """
        {
          "id": "evt-1",
          "webhookId": "wh-1",
          "shopId": "shop-1",
          "type": "UNKNOWN_EVENT_TYPE",
          "apiVersion": "2024-01-01",
          "createdAt": "2026-04-23T12:00:00Z",
          "testMode": false,
          "data": {}
        }
        """;

    private readonly FourthwallWebhookHandler _handler = new(new FourthwallWebhookSignatureVerifier());

    // ── No signing secret — verification skipped ──────────────────────────────

    [TestMethod]
    public async Task HandleAsync_NullSigningSecret_SkipsVerificationAndReturns200()
    {
        WebhookRequest request = BuildRequest(MinimalEnvelopeJson);
        FourthwallWebhookOptions options = new() { SigningSecret = null };

        WebhookHandleResult<FourthwallWebhookEvent> result =
            await _handler.HandleAsync(request, options);

        result.Response.StatusCode.Should().Be(200);
        result.IsAuthenticated.Should().BeTrue();
        result.Event.Should().NotBeNull();
    }

    [TestMethod]
    public async Task HandleAsync_EmptySigningSecret_SkipsVerificationAndReturns200()
    {
        WebhookRequest request = BuildRequest(MinimalEnvelopeJson);
        FourthwallWebhookOptions options = new() { SigningSecret = string.Empty };

        WebhookHandleResult<FourthwallWebhookEvent> result =
            await _handler.HandleAsync(request, options);

        result.Response.StatusCode.Should().Be(200);
        result.IsAuthenticated.Should().BeTrue();
        result.Event.Should().NotBeNull();
    }

    [TestMethod]
    public async Task HandleAsync_NullSigningSecret_NoSignatureHeader_StillSucceeds()
    {
        // Even without any signature header, null secret means verification is skipped.
        WebhookRequest request = BuildRequest(MinimalEnvelopeJson, includeSignatureHeader: false);
        FourthwallWebhookOptions options = new() { SigningSecret = null };

        WebhookHandleResult<FourthwallWebhookEvent> result =
            await _handler.HandleAsync(request, options);

        result.Response.StatusCode.Should().Be(200);
        result.Event.Should().NotBeNull();
    }

    [TestMethod]
    public async Task HandleAsync_NullSigningSecret_MalformedJson_Returns400()
    {
        // Skipping verification does not bypass JSON parsing.
        WebhookRequest request = BuildRequest("{ not valid json }", contentType: "application/json");
        FourthwallWebhookOptions options = new() { SigningSecret = null };

        WebhookHandleResult<FourthwallWebhookEvent> result =
            await _handler.HandleAsync(request, options);

        result.Response.StatusCode.Should().Be(400);
        // IsAuthenticated reflects that the auth/skip step passed — the failure is in content parsing.
        result.IsAuthenticated.Should().BeTrue();
        result.Event.Should().BeNull();
    }

    // ── With signing secret — verification enforced ───────────────────────────

    [TestMethod]
    public async Task HandleAsync_WithSigningSecret_ValidShopWebhookSignature_Returns200()
    {
        byte[] body = Encoding.UTF8.GetBytes(MinimalEnvelopeJson);
        string signature = ComputeHmacBase64(body, TestSigningSecret);

        WebhookRequest request = BuildRequest(
            MinimalEnvelopeJson,
            headerName: FourthwallWebhookSignatureVerifier.ShopWebhookSignatureHeaderName,
            headerValue: signature);

        FourthwallWebhookOptions options = new()
        {
            SigningSecret = TestSigningSecret,
            SignatureMode = FourthwallWebhookSignatureMode.ShopWebhook,
        };

        WebhookHandleResult<FourthwallWebhookEvent> result =
            await _handler.HandleAsync(request, options);

        result.Response.StatusCode.Should().Be(200);
        result.IsAuthenticated.Should().BeTrue();
        result.Event.Should().NotBeNull();
    }

    [TestMethod]
    public async Task HandleAsync_WithSigningSecret_ValidPlatformAppSignature_Returns200()
    {
        byte[] body = Encoding.UTF8.GetBytes(MinimalEnvelopeJson);
        string signature = ComputeHmacBase64(body, TestSigningSecret);

        WebhookRequest request = BuildRequest(
            MinimalEnvelopeJson,
            headerName: FourthwallWebhookSignatureVerifier.PlatformAppWebhookSignatureHeaderName,
            headerValue: signature);

        FourthwallWebhookOptions options = new()
        {
            SigningSecret = TestSigningSecret,
            SignatureMode = FourthwallWebhookSignatureMode.PlatformAppWebhook,
        };

        WebhookHandleResult<FourthwallWebhookEvent> result =
            await _handler.HandleAsync(request, options);

        result.Response.StatusCode.Should().Be(200);
        result.IsAuthenticated.Should().BeTrue();
    }

    [TestMethod]
    public async Task HandleAsync_WithSigningSecret_MissingSignatureHeader_Returns401()
    {
        WebhookRequest request = BuildRequest(MinimalEnvelopeJson, includeSignatureHeader: false);
        FourthwallWebhookOptions options = new() { SigningSecret = TestSigningSecret };

        WebhookHandleResult<FourthwallWebhookEvent> result =
            await _handler.HandleAsync(request, options);

        result.Response.StatusCode.Should().Be(401);
        result.IsAuthenticated.Should().BeFalse();
        result.Event.Should().BeNull();
    }

    [TestMethod]
    public async Task HandleAsync_WithSigningSecret_IncorrectSignature_Returns401()
    {
        WebhookRequest request = BuildRequest(
            MinimalEnvelopeJson,
            headerName: FourthwallWebhookSignatureVerifier.ShopWebhookSignatureHeaderName,
            headerValue: "aW52YWxpZA=="); // base64("invalid"), not a valid HMAC

        FourthwallWebhookOptions options = new() { SigningSecret = TestSigningSecret };

        WebhookHandleResult<FourthwallWebhookEvent> result =
            await _handler.HandleAsync(request, options);

        result.Response.StatusCode.Should().Be(401);
        result.IsAuthenticated.Should().BeFalse();
        result.Event.Should().BeNull();
    }

    [TestMethod]
    public async Task HandleAsync_WithSigningSecret_SignatureFromDifferentSecret_Returns401()
    {
        byte[] body = Encoding.UTF8.GetBytes(MinimalEnvelopeJson);
        string wrongSignature = ComputeHmacBase64(body, "wrong-secret");

        WebhookRequest request = BuildRequest(
            MinimalEnvelopeJson,
            headerName: FourthwallWebhookSignatureVerifier.ShopWebhookSignatureHeaderName,
            headerValue: wrongSignature);

        FourthwallWebhookOptions options = new() { SigningSecret = TestSigningSecret };

        WebhookHandleResult<FourthwallWebhookEvent> result =
            await _handler.HandleAsync(request, options);

        result.Response.StatusCode.Should().Be(401);
        result.IsAuthenticated.Should().BeFalse();
    }

    // ── Method / content-type guards (independent of signing secret) ──────────

    [TestMethod]
    [DataRow("GET")]
    [DataRow("PUT")]
    [DataRow("DELETE")]
    public async Task HandleAsync_NonPostMethod_Returns405(string method)
    {
        WebhookRequest request = BuildRequest(MinimalEnvelopeJson, method: method);
        FourthwallWebhookOptions options = new() { SigningSecret = null };

        WebhookHandleResult<FourthwallWebhookEvent> result =
            await _handler.HandleAsync(request, options);

        result.Response.StatusCode.Should().Be(405);
    }

    [TestMethod]
    public async Task HandleAsync_WrongContentType_Returns400()
    {
        WebhookRequest request = BuildRequest(MinimalEnvelopeJson, contentType: "text/plain");
        FourthwallWebhookOptions options = new() { SigningSecret = null };

        WebhookHandleResult<FourthwallWebhookEvent> result =
            await _handler.HandleAsync(request, options);

        result.Response.StatusCode.Should().Be(400);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static WebhookRequest BuildRequest(
        string json,
        string method = "POST",
        string contentType = "application/json",
        string? headerName = null,
        string? headerValue = null,
        bool includeSignatureHeader = true)
    {
        byte[] body = Encoding.UTF8.GetBytes(json);

        Dictionary<string, string[]> headers = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Content-Type"] = [contentType],
        };

        if (includeSignatureHeader && headerName is not null && headerValue is not null)
        {
            headers[headerName] = [headerValue];
        }

        return new WebhookRequest
        {
            Method = method,
            Path = "/webhooks/test",
            ContentType = contentType,
            Headers = headers,
            Body = body,
        };
    }

    private static string ComputeHmacBase64(byte[] body, string secret)
    {
        using HMACSHA256 hmac = new(Encoding.UTF8.GetBytes(secret));
        return Convert.ToBase64String(hmac.ComputeHash(body));
    }
}
