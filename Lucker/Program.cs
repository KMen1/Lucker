// See https://aka.ms/new-console-template for more information

using Discord;
using Discord.Addons.Hosting;
using Discord.Interactions;
using Discord.WebSocket;
using Lucker.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

var host = Host.CreateDefaultBuilder(args);

host.ConfigureServices((context, services) =>
    {
        services.AddDiscordHost((config, _) =>
        {
            config.SocketConfig = new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                AlwaysDownloadUsers = true,
                MessageCacheSize = 200,
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers,
                LogGatewayIntentWarnings = false,
                DefaultRetryMode = RetryMode.AlwaysFail
            };
            config.Token = context.Configuration["Discord:Token"] ?? throw new InvalidOperationException("Token not found");
        });
        services.AddInteractionService((config, _) =>
        {
            config.LogLevel = LogSeverity.Info;
            config.DefaultRunMode = RunMode.Async;
            config.UseCompiledLambda = true;
            config.LocalizationManager = new JsonLocalizationManager("/", "CommandLocale");
        });
        services.AddHostedService<InteractionHandler>();
        services.AddSingleton<GameService>();

    });
    
host.UseSerilog((_, configuration) =>
    {
        configuration.Enrich
            .FromLogContext()
            .MinimumLevel.Debug()
            .WriteTo.Console();
    },
    preserveStaticLogger: true);

await host.RunConsoleAsync();