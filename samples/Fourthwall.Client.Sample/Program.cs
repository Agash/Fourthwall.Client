using DevTunnels.Client;
using Fourthwall.Client.AspNetCore;
using Fourthwall.Client.DependencyInjection;
using Fourthwall.Client.Events;
using Fourthwall.Client.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Spectre.Console;
using System.Collections.Concurrent;

CancellationTokenSource shutdown = new();

Console.CancelKeyPress += static (_, e) =>
{
    e.Cancel = true;
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

        AnsiConsole.MarkupLine("[grey]Fourthwall Platform API + webhook sample with ASP.NET Core, Spectre.Console, and DevTunnels.Client.[/]");
        AnsiConsole.WriteLine();

        SampleConfiguration configuration = PromptConfiguration();

        ConcurrentQueue<FourthwallWebhookEvent> receivedEvents = new();
        object consoleLock = new();

        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        _ = builder.WebHost.UseUrls($"http://127.0.0.1:{configuration.LocalPort}");
        _ = builder.Services.AddFourthwallClient();

        WebApplication app = builder.Build();

        _ = app.MapGet(
            "/",
            () => Results.Text(
                "Fourthwall.Client.Sample is running.\n" +
                "POST Fourthwall webhook payloads to the configured route.\n",
                "text/plain"));

        _ = app.MapFourthwallWebhook(
            configuration.WebhookPath,
            static (context, _) =>
            {
                FourthwallWebhookOptions options = (FourthwallWebhookOptions)context.Items["FourthwallWebhookOptions"]!;
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

        _ = app.Use(async (context, next) =>
        {
            context.Items["FourthwallWebhookOptions"] = new FourthwallWebhookOptions
            {
                SigningSecret = configuration.SigningSecret,
                SignatureMode = configuration.SignatureMode,
            };

            await next(context).ConfigureAwait(false);
        });

        string localBaseUrl = $"http://127.0.0.1:{configuration.LocalPort}";

        await app.StartAsync(cancellationToken).ConfigureAwait(false);

        RenderStartupSummary(configuration, localBaseUrl);

        DevTunnelsRuntime? devTunnelsRuntime = null;

        if (configuration.UseDevTunnels)
        {
            devTunnelsRuntime = await StartDevTunnelsAsync(configuration, cancellationToken).ConfigureAwait(false);
            RenderTunnelSummary(configuration, devTunnelsRuntime.PublicBaseUrl);
        }

        RenderUsageInstructions(configuration, localBaseUrl, devTunnelsRuntime?.PublicBaseUrl);

        await RunCommandLoopAsync(configuration, receivedEvents, devTunnelsRuntime, consoleLock, cancellationToken)
            .ConfigureAwait(false);

        if (devTunnelsRuntime is not null)
        {
            await devTunnelsRuntime.StopAsync(CancellationToken.None).ConfigureAwait(false);
        }

        await app.StopAsync(CancellationToken.None).ConfigureAwait(false);
        await app.DisposeAsync().ConfigureAwait(false);
    }

    private static SampleConfiguration PromptConfiguration()
    {
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

        string signingSecret = AnsiConsole.Prompt(
            new TextPrompt<string>("Fourthwall [green]signing secret[/]?")
                .PromptStyle("deepskyblue1")
                .Secret());

        FourthwallWebhookSignatureMode signatureMode = AnsiConsole.Prompt(
            new SelectionPrompt<FourthwallWebhookSignatureMode>()
                .Title("Webhook [green]signature mode[/]?")
                .AddChoices(
                    FourthwallWebhookSignatureMode.ShopWebhook,
                    FourthwallWebhookSignatureMode.PlatformAppWebhook));

        bool useDevTunnels = AnsiConsole.Confirm("Use [green]Azure Dev Tunnels[/] for a public HTTPS URL?", true);

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
            LoginProvider: loginProvider);
    }

    private static async Task<DevTunnelsRuntime> StartDevTunnelsAsync(
        SampleConfiguration configuration,
        CancellationToken cancellationToken)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Azure Dev Tunnels walkthrough[/]");
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

        _ = await client.EnsureLoggedInAsync(configuration.LoginProvider, cancellationToken).ConfigureAwait(false);

        _ = await client.CreateOrUpdateTunnelAsync(
            configuration.TunnelId,
            new DevTunnelOptions
            {
                Description = "Fourthwall.Client.Sample tunnel",
                AllowAnonymous = true,
            },
            cancellationToken).ConfigureAwait(false);

        _ = await client.CreateOrReplacePortAsync(
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

    private static void RenderStartupSummary(SampleConfiguration configuration, string localBaseUrl)
    {
        string localWebhookUrl = CombineUrl(localBaseUrl, configuration.WebhookPath);

        Table table = new Table()
            .RoundedBorder()
            .BorderColor(Color.CornflowerBlue)
            .AddColumn("[bold]Setting[/]")
            .AddColumn("[bold]Value[/]");

        _ = table.AddRow("Local base URL", $"[white]{Markup.Escape(localBaseUrl)}[/]");
        _ = table.AddRow("Webhook path", $"[white]{Markup.Escape(configuration.WebhookPath)}[/]");
        _ = table.AddRow("Local webhook URL", $"[white]{Markup.Escape(localWebhookUrl)}[/]");
        _ = table.AddRow("Signing secret", "[grey](hidden)[/]");
        _ = table.AddRow("Signature mode", $"[white]{Markup.Escape(configuration.SignatureMode.ToString())}[/]");
        _ = table.AddRow("Dev Tunnels enabled", configuration.UseDevTunnels ? "[green]yes[/]" : "[yellow]no[/]");

        AnsiConsole.Write(new Panel(table)
            .Header("[bold]Local runtime[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.CornflowerBlue));
    }

    private static void RenderTunnelSummary(SampleConfiguration configuration, Uri publicBaseUrl)
    {
        string publicWebhookUrl = CombineUrl(publicBaseUrl.ToString().TrimEnd('/'), configuration.WebhookPath);

        Table table = new Table()
            .RoundedBorder()
            .BorderColor(Color.Green)
            .AddColumn("[bold]Setting[/]")
            .AddColumn("[bold]Value[/]");

        _ = table.AddRow("Tunnel ID", $"[white]{Markup.Escape(configuration.TunnelId)}[/]");
        _ = table.AddRow("Public base URL", $"[white]{Markup.Escape(publicBaseUrl.ToString())}[/]");
        _ = table.AddRow("Public webhook URL", $"[white]{Markup.Escape(publicWebhookUrl)}[/]");

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
            new Markup("[bold]Walkthrough[/]"),
            new Text(string.Empty),
            new Markup("1. Start this sample and keep it running."),
            new Markup("2. Copy the webhook URL into your Fourthwall webhook configuration."),
            new Markup("3. Use the exact signing secret configured in Fourthwall."),
            new Markup("4. Send a test webhook or trigger a real event."),
            new Text(string.Empty),
            new Markup($"[grey]Local webhook URL:[/] [white]{Markup.Escape(localWebhookUrl)}[/]"),
            publicWebhookUrl is not null
                ? new Markup($"[grey]Public webhook URL:[/] [white]{Markup.Escape(publicWebhookUrl)}[/]")
                : new Markup("[grey]Public webhook URL:[/] [yellow](Dev Tunnels disabled)[/]"));

        AnsiConsole.Write(new Panel(rows)
            .Header("[bold]How to use the sample[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Blue));
    }

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

                        _ = table.AddRow("Local", $"[white]{Markup.Escape(localWebhookUrl)}[/]");

                        if (devTunnelsRuntime is not null)
                        {
                            string publicWebhookUrl = CombineUrl(
                                devTunnelsRuntime.PublicBaseUrl.ToString().TrimEnd('/'),
                                configuration.WebhookPath);

                            _ = table.AddRow("Public", $"[white]{Markup.Escape(publicWebhookUrl)}[/]");
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
                            _ = table.AddRow(
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
                        string headerName = configuration.SignatureMode switch
                        {
                            FourthwallWebhookSignatureMode.ShopWebhook => "X-Fourthwall-Hmac-SHA256",
                            FourthwallWebhookSignatureMode.PlatformAppWebhook => "X-Fourthwall-Hmac-Apps-SHA256",
                            _ => "(unknown)"
                        };

                        AnsiConsole.MarkupLineInterpolated($"[grey]Expected signature header:[/] [white]{Markup.Escape(headerName)}[/]");
                    }

                    break;

                case "Exit":
                    return;
            }

            await Task.Yield();
        }
    }

    private static void RenderReceivedEvent(FourthwallWebhookEvent evt)
    {
        Grid grid = new();
        _ = grid.AddColumn();
        _ = grid.AddColumn();

        _ = grid.AddRow("[bold]Type[/]", Markup.Escape(evt.Type));
        _ = grid.AddRow("[bold]Shop[/]", Markup.Escape(evt.ShopId));
        _ = grid.AddRow("[bold]Webhook[/]", Markup.Escape(evt.WebhookId));
        _ = grid.AddRow("[bold]Created[/]", Markup.Escape(evt.CreatedAt.ToString("u")));
        _ = grid.AddRow("[bold]Test Mode[/]", evt.TestMode ? "[yellow]yes[/]" : "[green]no[/]");

        AnsiConsole.Write(new Panel(grid)
            .Header("[bold green]Webhook event received[/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Green));
    }

    private static string CombineUrl(string baseUrl, string path)
    {
        string normalizedBase = baseUrl.TrimEnd('/');
        string normalizedPath = path.StartsWith('/') ? path : "/" + path;
        return normalizedBase + normalizedPath;
    }

    private sealed record SampleConfiguration(
        int LocalPort,
        string WebhookPath,
        string SigningSecret,
        FourthwallWebhookSignatureMode SignatureMode,
        bool UseDevTunnels,
        string TunnelId,
        LoginProvider LoginProvider);

    private sealed class DevTunnelsRuntime(dynamic session, Uri publicBaseUrl)
    {
        public dynamic Session { get; } = session;

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