using DevTunnels.Client;
using DevTunnels.Client.Authentication;
using DevTunnels.Client.Hosting;
using DevTunnels.Client.Ports;
using DevTunnels.Client.Tunnels;
using Fourthwall.Client;
using Fourthwall.Client.AspNetCore;
using Fourthwall.Client.DependencyInjection;
using Fourthwall.Client.Events;
using Fourthwall.Client.Generated;
using Fourthwall.Client.Generated.Models.App.Openapi.Endpoint.OpenApiWebhookConfigurationEndpoint;
using Fourthwall.Client.Generated.Models.Openapi.Model;
using Fourthwall.Client.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Spectre.Console;
using System.Collections.Concurrent;

CancellationTokenSource shutdown = new();

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    shutdown.Cancel();
};

try
{
    await SampleApplication.RunAsync(shutdown.Token).ConfigureAwait(false);
}
catch (OperationCanceledException)
{
    // Normal shutdown path.
}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex);
    Environment.ExitCode = 1;
}

internal static class SampleApplication
{
    public static async Task RunAsync(CancellationToken cancellationToken)
    {
        AnsiConsole.Clear();

        AnsiConsole.Write(
            new FigletText("Fourthwall Sample")
                .Color(Color.CornflowerBlue));

        AnsiConsole.MarkupLine("[grey]Fourthwall Platform API + webhook sample with ASP.NET Core, auto-registration, and Azure Dev Tunnels.[/]");
        AnsiConsole.WriteLine();

        SampleConfiguration configuration = PromptConfiguration();

        ConcurrentQueue<FourthwallWebhookEvent> receivedEvents = new();
        object consoleLock = new();

        // ── Build ASP.NET Core app ────────────────────────────────────────────

        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls($"http://127.0.0.1:{configuration.LocalPort}");
        builder.Services.AddFourthwallClient();

        WebApplication app = builder.Build();

        app.MapGet(
            "/",
            () => Results.Text(
                "Fourthwall.Client.Sample is running.\n" +
                "POST Fourthwall webhook payloads to the configured route.\n",
                "text/plain"));

        app.MapFourthwallWebhook(
            configuration.WebhookPath,
            (context, _) =>
            {
                FourthwallWebhookOptions options = new()
                {
                    SigningSecret = configuration.SigningSecret,
                    SignatureMode = configuration.SignatureMode,
                };
                return Task.FromResult(options);
            },
            async (evt, _, _) =>
            {
                receivedEvents.Enqueue(evt);

                lock (consoleLock)
                {
                    RenderReceivedEvent(evt);
                }

                await Task.CompletedTask.ConfigureAwait(false);
            },
            async (result, httpContext, _) =>
            {
                lock (consoleLock)
                {
                    string remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    string requestId = httpContext.TraceIdentifier;

                    string auth = result.IsAuthenticated ? "[green]yes[/]" : "[red]no[/]";
                    string known = result.IsKnownEvent ? "[green]yes[/]" : "[yellow]no[/]";
                    string status = $"[blue]{result.Response.StatusCode}[/]";

                    AnsiConsole.MarkupLineInterpolated(
                        $"[grey]Request[/] [white]{Markup.Escape(requestId)}[/] from [white]{Markup.Escape(remoteIp)}[/] -> status {status}, authenticated {auth}, known event {known}.");

                    if (!string.IsNullOrWhiteSpace(result.FailureReason))
                    {
                        AnsiConsole.MarkupLineInterpolated($"[yellow]Reason:[/] {Markup.Escape(result.FailureReason)}");
                    }
                }

                await Task.CompletedTask.ConfigureAwait(false);
            });

        string localBaseUrl = $"http://127.0.0.1:{configuration.LocalPort}";

        await app.StartAsync(cancellationToken).ConfigureAwait(false);

        RenderStartupSummary(configuration, localBaseUrl);

        // ── Dev Tunnels ───────────────────────────────────────────────────────

        DevTunnelsRuntime? devTunnelsRuntime = null;
        string? publicWebhookUrl = null;

        if (configuration.UseDevTunnels)
        {
            devTunnelsRuntime = await StartDevTunnelsAsync(configuration, cancellationToken).ConfigureAwait(false);
            publicWebhookUrl = CombineUrl(devTunnelsRuntime.PublicBaseUrl.ToString().TrimEnd('/'), configuration.WebhookPath);
            RenderTunnelSummary(configuration, devTunnelsRuntime.PublicBaseUrl, publicWebhookUrl);
        }

        // ── Webhook auto-registration ─────────────────────────────────────────

        string? registeredWebhookId = null;

        if (configuration.ApiCredentials is not null && publicWebhookUrl is not null)
        {
            registeredWebhookId = await RegisterWebhookAsync(
                configuration.ApiCredentials,
                publicWebhookUrl,
                consoleLock,
                cancellationToken).ConfigureAwait(false);
        }
        else if (configuration.ApiCredentials is not null && publicWebhookUrl is null)
        {
            AnsiConsole.MarkupLine("[yellow]Skipping webhook auto-registration — no public URL (Dev Tunnels disabled).[/]");
        }

        RenderUsageInstructions(configuration, localBaseUrl, devTunnelsRuntime?.PublicBaseUrl);

        // ── Command loop ──────────────────────────────────────────────────────

        await RunCommandLoopAsync(
            configuration,
            receivedEvents,
            devTunnelsRuntime,
            consoleLock,
            cancellationToken).ConfigureAwait(false);

        // ── Cleanup ───────────────────────────────────────────────────────────

        if (registeredWebhookId is not null && configuration.ApiCredentials is not null)
        {
            await DeregisterWebhookAsync(
                configuration.ApiCredentials,
                registeredWebhookId,
                consoleLock).ConfigureAwait(false);
        }

        if (devTunnelsRuntime is not null)
        {
            await devTunnelsRuntime.StopAsync(CancellationToken.None).ConfigureAwait(false);
        }

        await app.StopAsync(CancellationToken.None).ConfigureAwait(false);
        await app.DisposeAsync().ConfigureAwait(false);
    }

    // ── Configuration prompt ──────────────────────────────────────────────────

    private static SampleConfiguration PromptConfiguration()
    {
        AnsiConsole.MarkupLine("[bold]Step 1 — API credentials (for auto-registration)[/]");
        AnsiConsole.MarkupLine("[grey]Leave blank to skip webhook auto-registration and manage webhooks manually via the Fourthwall dashboard.[/]");
        AnsiConsole.WriteLine();

        string apiUsername = AnsiConsole.Prompt(
            new TextPrompt<string>("Fourthwall [green]API username[/]?")
                .AllowEmpty());

        ApiCredentials? apiCredentials = null;

        if (!string.IsNullOrWhiteSpace(apiUsername))
        {
            string apiPassword = AnsiConsole.Prompt(
                new TextPrompt<string>("Fourthwall [green]API password[/]?")
                    .PromptStyle("deepskyblue1")
                    .Secret());

            apiCredentials = new ApiCredentials(apiUsername.Trim(), apiPassword);
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Step 2 — Webhook endpoint[/]");
        AnsiConsole.WriteLine();

        int localPort = AnsiConsole.Prompt(
            new TextPrompt<int>("Local [green]HTTP port[/]?")
                .DefaultValue(5074)
                .Validate(port => port is > 0 and <= 65535
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red]Port must be between 1 and 65535.[/]")));

        string webhookPath = AnsiConsole.Prompt(
            new TextPrompt<string>("Webhook [green]path[/]?")
                .DefaultValue("/webhooks/fourthwall/events")
                .AllowEmpty());

        if (string.IsNullOrWhiteSpace(webhookPath))
        {
            webhookPath = "/webhooks/fourthwall/events";
        }

        if (!webhookPath.StartsWith('/'))
        {
            webhookPath = "/" + webhookPath;
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Step 3 — Signature verification (optional)[/]");
        AnsiConsole.MarkupLine("[grey]When omitted, all payloads arriving at your endpoint are accepted without HMAC verification.[/]");
        AnsiConsole.WriteLine();

        bool useSigningSecret = AnsiConsole.Confirm("Verify webhook signatures with a [green]signing secret[/]?", false);

        string? signingSecret = null;
        FourthwallWebhookSignatureMode signatureMode = FourthwallWebhookSignatureMode.ShopWebhook;

        if (useSigningSecret)
        {
            signingSecret = AnsiConsole.Prompt(
                new TextPrompt<string>("Fourthwall [green]signing secret[/]?")
                    .PromptStyle("deepskyblue1")
                    .Secret());

            signatureMode = AnsiConsole.Prompt(
                new SelectionPrompt<FourthwallWebhookSignatureMode>()
                    .Title("Webhook [green]signature mode[/]?")
                    .AddChoices(
                        FourthwallWebhookSignatureMode.ShopWebhook,
                        FourthwallWebhookSignatureMode.PlatformAppWebhook));
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Step 4 — Azure Dev Tunnels[/]");
        AnsiConsole.WriteLine();

        bool useDevTunnels = AnsiConsole.Confirm("Use [green]Azure Dev Tunnels[/] for a public HTTPS URL?", defaultValue: true);

        string tunnelId = "fourthwall-client-sample";
        LoginProvider loginProvider = LoginProvider.GitHub;

        if (useDevTunnels)
        {
            tunnelId = AnsiConsole.Prompt(
                new TextPrompt<string>("Dev Tunnel [green]tunnel ID[/]?")
                    .DefaultValue("fourthwall-client-sample")
                    .AllowEmpty());

            if (string.IsNullOrWhiteSpace(tunnelId))
            {
                tunnelId = "fourthwall-client-sample";
            }

            loginProvider = AnsiConsole.Prompt(
                new SelectionPrompt<LoginProvider>()
                    .Title("Login provider for [green]devtunnel[/]?")
                    .AddChoices(LoginProvider.GitHub, LoginProvider.Microsoft));
        }

        return new SampleConfiguration(
            LocalPort: localPort,
            WebhookPath: webhookPath,
            SigningSecret: signingSecret,
            SignatureMode: signatureMode,
            UseDevTunnels: useDevTunnels,
            TunnelId: tunnelId,
            LoginProvider: loginProvider,
            ApiCredentials: apiCredentials);
    }

    // ── Dev Tunnels ───────────────────────────────────────────────────────────

    private static async Task<DevTunnelsRuntime> StartDevTunnelsAsync(
        SampleConfiguration configuration,
        CancellationToken cancellationToken)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Starting Azure Dev Tunnels...[/]");
        AnsiConsole.WriteLine();

        DevTunnelsClient client = new(new DevTunnelsClientOptions
        {
            CommandTimeout = TimeSpan.FromSeconds(20),
        });

        DevTunnelCliProbeResult probe = await client.ProbeCliAsync(cancellationToken).ConfigureAwait(false);

        if (!probe.IsInstalled)
        {
            throw new InvalidOperationException(
                "The devtunnel CLI is not installed or could not be found. Install it first, then re-run the sample.");
        }

        AnsiConsole.MarkupLineInterpolated($"[green]CLI found:[/] devtunnel [white]{Markup.Escape(probe.Version?.ToString() ?? "unknown")}[/]");

        await client.EnsureLoggedInAsync(configuration.LoginProvider, cancellationToken).ConfigureAwait(false);

        await client.CreateOrUpdateTunnelAsync(
            configuration.TunnelId,
            new DevTunnelOptions
            {
                Description = "Fourthwall.Client.Sample tunnel",
                AllowAnonymous = true,
            },
            cancellationToken).ConfigureAwait(false);

        await client.CreateOrReplacePortAsync(
            configuration.TunnelId,
            configuration.LocalPort,
            new DevTunnelPortOptions
            {
                Protocol = "http",
            },
            cancellationToken).ConfigureAwait(false);

        IDevTunnelHostSession session = await client.StartHostSessionAsync(
            new DevTunnelHostStartOptions
            {
                TunnelId = configuration.TunnelId,
            },
            cancellationToken).ConfigureAwait(false);

        await session.WaitForReadyAsync(cancellationToken).ConfigureAwait(false);

        Uri publicBaseUrl = session.PublicUrl
            ?? throw new InvalidOperationException("The Dev Tunnel host session became ready without a public URL.");

        return new DevTunnelsRuntime(session, publicBaseUrl);
    }

    // ── Webhook registration ──────────────────────────────────────────────────

    private static async Task<string?> RegisterWebhookAsync(
        ApiCredentials credentials,
        string publicWebhookUrl,
        object consoleLock,
        CancellationToken cancellationToken)
    {
        lock (consoleLock)
        {
            AnsiConsole.MarkupLine("[bold]Registering webhook with Fourthwall...[/]");
        }

        FourthwallClientFactory factory = new();
        FourthwallApiClient apiClient = factory.CreateWithBasicAuth(new FourthwallBasicAuthOptions
        {
            Username = credentials.Username,
            Password = credentials.Password,
        });

        try
        {
            // Check if a webhook for this URL already exists.
            var existing = await apiClient.OpenApi.V10.Webhooks.GetAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            WebhookConfigurationV1? match = existing?.Results?
                .FirstOrDefault(w => string.Equals(w.Url, publicWebhookUrl, StringComparison.OrdinalIgnoreCase));

            if (match is not null)
            {
                lock (consoleLock)
                {
                    AnsiConsole.MarkupLineInterpolated(
                        $"[green]Existing webhook found:[/] [white]{Markup.Escape(match.Id ?? "(no id)")}[/] — reusing.");
                }

                return match.Id;
            }

            // Register a new webhook for all supported event types.
            WebhookConfigurationV1? created = await apiClient.OpenApi.V10.Webhooks
                .PostAsync(
                    new WebhookConfigurationCreateRequest
                    {
                        Url = publicWebhookUrl,
                        AllowedTypes =
                        [
                            WebhookConfigurationCreateRequest_allowedTypes.ORDER_PLACED,
                            WebhookConfigurationCreateRequest_allowedTypes.ORDER_UPDATED,
                            WebhookConfigurationCreateRequest_allowedTypes.GIFT_PURCHASE,
                            WebhookConfigurationCreateRequest_allowedTypes.DONATION,
                            WebhookConfigurationCreateRequest_allowedTypes.SUBSCRIPTION_PURCHASED,
                            WebhookConfigurationCreateRequest_allowedTypes.SUBSCRIPTION_EXPIRED,
                            WebhookConfigurationCreateRequest_allowedTypes.SUBSCRIPTION_CHANGED,
                        ],
                    },
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (created is null)
            {
                lock (consoleLock)
                {
                    AnsiConsole.MarkupLine("[red]Webhook registration returned an empty response.[/]");
                }

                return null;
            }

            lock (consoleLock)
            {
                AnsiConsole.MarkupLineInterpolated(
                    $"[green]Webhook registered:[/] id=[white]{Markup.Escape(created.Id ?? "(no id)")}[/]");
                AnsiConsole.MarkupLineInterpolated(
                    $"[grey]Endpoint:[/] [white]{Markup.Escape(publicWebhookUrl)}[/]");
            }

            return created.Id;
        }
        catch (Exception ex)
        {
            lock (consoleLock)
            {
                AnsiConsole.MarkupLine($"[red]Webhook registration failed:[/] {Markup.Escape(ex.Message)}");
                AnsiConsole.MarkupLine("[yellow]You can register the webhook manually via the Fourthwall dashboard.[/]");
            }

            return null;
        }
    }

    private static async Task DeregisterWebhookAsync(
        ApiCredentials credentials,
        string webhookId,
        object consoleLock)
    {
        bool shouldDelete = false;

        lock (consoleLock)
        {
            shouldDelete = AnsiConsole.Confirm(
                $"Delete the registered webhook [white]{Markup.Escape(webhookId)}[/] from Fourthwall?",
                defaultValue: true);
        }

        if (!shouldDelete)
        {
            return;
        }

        FourthwallClientFactory factory = new();
        FourthwallApiClient apiClient = factory.CreateWithBasicAuth(new FourthwallBasicAuthOptions
        {
            Username = credentials.Username,
            Password = credentials.Password,
        });

        try
        {
            await apiClient.OpenApi.V10.Webhooks[webhookId]
                .DeleteAsync(cancellationToken: CancellationToken.None)
                .ConfigureAwait(false);

            lock (consoleLock)
            {
                AnsiConsole.MarkupLineInterpolated($"[green]Webhook [white]{Markup.Escape(webhookId)}[/] deleted.[/]");
            }
        }
        catch (Exception ex)
        {
            lock (consoleLock)
            {
                AnsiConsole.MarkupLine($"[red]Webhook deletion failed:[/] {Markup.Escape(ex.Message)}");
            }
        }
    }

    // ── Rendering ─────────────────────────────────────────────────────────────

    private static void RenderStartupSummary(SampleConfiguration configuration, string localBaseUrl)
    {
        string localWebhookUrl = CombineUrl(localBaseUrl, configuration.WebhookPath);

        Table table = new Table()
            .RoundedBorder()
            .BorderColor(Color.CornflowerBlue)
            .AddColumn("[bold]Setting[/]")
            .AddColumn("[bold]Value[/]");

        table.AddRow("Local base URL", $"[white]{Markup.Escape(localBaseUrl)}[/]");
        table.AddRow("Webhook path", $"[white]{Markup.Escape(configuration.WebhookPath)}[/]");
        table.AddRow("Local webhook URL", $"[white]{Markup.Escape(localWebhookUrl)}[/]");
        table.AddRow("API credentials", configuration.ApiCredentials is not null ? "[green]provided[/]" : "[yellow]not provided (manual registration)[/]");
        table.AddRow("Signature verification",
            configuration.SigningSecret is not null ? "[green]enabled[/]" : "[yellow]disabled (no secret)[/]");
        if (configuration.SigningSecret is not null)
        {
            table.AddRow("Signature mode", $"[white]{Markup.Escape(configuration.SignatureMode.ToString())}[/]");
        }
        table.AddRow("Dev Tunnels enabled", configuration.UseDevTunnels ? "[green]yes[/]" : "[yellow]no[/]");

        AnsiConsole.Write(new Panel(table)
            .Header("[bold]Runtime configuration[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.CornflowerBlue));
    }

    private static void RenderTunnelSummary(SampleConfiguration configuration, Uri publicBaseUrl, string publicWebhookUrl)
    {
        Table table = new Table()
            .RoundedBorder()
            .BorderColor(Color.Green)
            .AddColumn("[bold]Setting[/]")
            .AddColumn("[bold]Value[/]");

        table.AddRow("Tunnel ID", $"[white]{Markup.Escape(configuration.TunnelId)}[/]");
        table.AddRow("Public base URL", $"[white]{Markup.Escape(publicBaseUrl.ToString())}[/]");
        table.AddRow("Public webhook URL", $"[white]{Markup.Escape(publicWebhookUrl)}[/]");

        AnsiConsole.Write(new Panel(table)
            .Header("[bold]Public tunnel[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Green));
    }

    private static void RenderUsageInstructions(
        SampleConfiguration configuration,
        string localBaseUrl,
        Uri? publicBaseUrl)
    {
        string localWebhookUrl = CombineUrl(localBaseUrl, configuration.WebhookPath);
        string? publicWebhookUrl = publicBaseUrl is null
            ? null
            : CombineUrl(publicBaseUrl.ToString().TrimEnd('/'), configuration.WebhookPath);

        Rows rows = new(
            new Markup("[bold]Next steps[/]"),
            new Text(string.Empty),
            new Markup("1. If auto-registration succeeded, send a test webhook from your Fourthwall dashboard."),
            new Markup("2. Otherwise, copy the public webhook URL and register it manually in Fourthwall."),
            new Markup("3. Events will be printed here as they arrive."),
            new Markup("4. Use Ctrl+C or the Exit command to shut down cleanly."),
            new Text(string.Empty),
            new Markup($"[grey]Local webhook URL:[/] [white]{Markup.Escape(localWebhookUrl)}[/]"),
            publicWebhookUrl is not null
                ? new Markup($"[grey]Public webhook URL:[/] [white]{Markup.Escape(publicWebhookUrl)}[/]")
                : new Markup("[grey]Public webhook URL:[/] [yellow](Dev Tunnels disabled)[/]"));

        AnsiConsole.Write(new Panel(rows)
            .Header("[bold]How to use[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Blue));
    }

    // ── Command loop ──────────────────────────────────────────────────────────

    private static async Task RunCommandLoopAsync(
        SampleConfiguration configuration,
        ConcurrentQueue<FourthwallWebhookEvent> receivedEvents,
        DevTunnelsRuntime? devTunnelsRuntime,
        object consoleLock,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            AnsiConsole.WriteLine();

            string command = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Choose an action[/]")
                    .AddChoices(
                        "Show webhook URLs",
                        "Show recent events",
                        "Show signature header name",
                        "Exit"));

            switch (command)
            {
                case "Show webhook URLs":
                    lock (consoleLock)
                    {
                        string localBaseUrl = $"http://127.0.0.1:{configuration.LocalPort}";
                        string localWebhookUrl = CombineUrl(localBaseUrl, configuration.WebhookPath);

                        Table table = new Table()
                            .RoundedBorder()
                            .AddColumn("[bold]Endpoint[/]")
                            .AddColumn("[bold]URL[/]");

                        table.AddRow("Local", $"[white]{Markup.Escape(localWebhookUrl)}[/]");

                        if (devTunnelsRuntime is not null)
                        {
                            string publicWebhookUrl = CombineUrl(
                                devTunnelsRuntime.PublicBaseUrl.ToString().TrimEnd('/'),
                                configuration.WebhookPath);

                            table.AddRow("Public", $"[white]{Markup.Escape(publicWebhookUrl)}[/]");
                        }

                        AnsiConsole.Write(table);
                    }

                    break;

                case "Show recent events":
                    lock (consoleLock)
                    {
                        if (receivedEvents.IsEmpty)
                        {
                            AnsiConsole.MarkupLine("[yellow]No events have been received yet.[/]");
                            break;
                        }

                        FourthwallWebhookEvent[] snapshot = [.. receivedEvents];

                        Table table = new Table()
                            .RoundedBorder()
                            .AddColumn("[bold]Type[/]")
                            .AddColumn("[bold]Shop[/]")
                            .AddColumn("[bold]Created[/]")
                            .AddColumn("[bold]Test Mode[/]");

                        foreach (FourthwallWebhookEvent evt in snapshot.TakeLast(20))
                        {
                            table.AddRow(
                                Markup.Escape(evt.Type),
                                Markup.Escape(evt.ShopId),
                                Markup.Escape(evt.CreatedAt.ToString("u")),
                                evt.TestMode ? "[yellow]yes[/]" : "[green]no[/]");
                        }

                        AnsiConsole.Write(table);
                    }

                    break;

                case "Show signature header name":
                    lock (consoleLock)
                    {
                        if (configuration.SigningSecret is null)
                        {
                            AnsiConsole.MarkupLine("[yellow]Signature verification is disabled — no header is checked.[/]");
                        }
                        else
                        {
                            string headerName = configuration.SignatureMode switch
                            {
                                FourthwallWebhookSignatureMode.ShopWebhook => "X-Fourthwall-Hmac-SHA256",
                                FourthwallWebhookSignatureMode.PlatformAppWebhook => "X-Fourthwall-Hmac-Apps-SHA256",
                                _ => "(unknown)"
                            };

                            AnsiConsole.MarkupLineInterpolated($"[grey]Expected signature header:[/] [white]{Markup.Escape(headerName)}[/]");
                        }
                    }

                    break;

                case "Exit":
                    return;
            }

            await Task.Yield();
        }
    }

    // ── Event rendering ───────────────────────────────────────────────────────

    private static void RenderReceivedEvent(FourthwallWebhookEvent evt)
    {
        Grid grid = new();
        grid.AddColumn();
        grid.AddColumn();

        grid.AddRow("[bold]Type[/]", Markup.Escape(evt.Type));
        grid.AddRow("[bold]Shop[/]", Markup.Escape(evt.ShopId));
        grid.AddRow("[bold]Webhook[/]", Markup.Escape(evt.WebhookId));
        grid.AddRow("[bold]Created[/]", Markup.Escape(evt.CreatedAt.ToString("u")));
        grid.AddRow("[bold]Test Mode[/]", evt.TestMode ? "[yellow]yes[/]" : "[green]no[/]");

        AnsiConsole.Write(new Panel(grid)
            .Header("[bold green]Webhook event received[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Green));
    }

    // ── Utilities ─────────────────────────────────────────────────────────────

    private static string CombineUrl(string baseUrl, string path)
    {
        string normalizedBase = baseUrl.TrimEnd('/');
        string normalizedPath = path.StartsWith('/') ? path : "/" + path;
        return normalizedBase + normalizedPath;
    }

    // ── Records and support types ─────────────────────────────────────────────

    private sealed record SampleConfiguration(
        int LocalPort,
        string WebhookPath,
        string? SigningSecret,
        FourthwallWebhookSignatureMode SignatureMode,
        bool UseDevTunnels,
        string TunnelId,
        LoginProvider LoginProvider,
        ApiCredentials? ApiCredentials);

    private sealed record ApiCredentials(string Username, string Password);

    private sealed class DevTunnelsRuntime(IDevTunnelHostSession session, Uri publicBaseUrl)
    {
        public IDevTunnelHostSession Session { get; } = session;

        public Uri PublicBaseUrl { get; } = publicBaseUrl;

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Session.StopAsync(cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // Best-effort shutdown for the sample.
            }
        }
    }
}
