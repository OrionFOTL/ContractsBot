using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MafiaContractsBot.Extensions;
using MafiaContractsBot.Features.ContractsRanking;

namespace MafiaContractsBot;

public class BotWorker(
    ILogger<BotWorker> logger,
    IConfiguration configuration,
    DiscordSocketClient client,
    InteractionService interactionService,
    IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting bot...");

        client.Log += Log;
        client.Ready += RegisterCommandsToServers;
        client.InteractionCreated += HandleInteraction;
        interactionService.Log += Log;
        interactionService.SlashCommandExecuted += SlashCommandExecuted;

        await RegisterModules();

        var token = configuration["Token"];
        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();
    }

    public override async Task StopAsync(CancellationToken cancellationToken) => await client.StopAsync();

    private async Task RegisterModules()
    {
        using var scope = serviceProvider.CreateScope();
        await interactionService.AddModulesAsync(typeof(Program).Assembly, scope.ServiceProvider);
    }

    private async Task RegisterCommandsToServers()
    {
        var guilds = new ulong[] { 1340692574733598831, 887064391445512294 };

        foreach (var id in guilds)
        {
            await interactionService.RegisterCommandsToGuildAsync(id);
        }
    }

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        logger.LogInformation("Interaction {type} from {user}", interaction.Type, interaction.User);

        var interactionContext = new SocketInteractionContext(client, interaction);
        await interactionService.ExecuteCommandAsync(interactionContext, serviceProvider);
    }

    private async Task SlashCommandExecuted(SlashCommandInfo commandInfo, IInteractionContext context, IResult result)
    {
        if (result.IsSuccess)
        {
            return;
        }

        var embed = new EmbedBuilder()
            .WithTitle("B³¹d")
            .WithColor(Color.Red)
            .WithDescription(result.ErrorReason);

        if (result is ExecuteResult { Error: InteractionCommandError.Exception, Exception: not null } executeResult)
        {
            embed = executeResult.Exception is ContractsDomainException
                ? embed.WithDescription(executeResult.Exception.Message)
                : embed.WithDescription(executeResult.Exception.Message + Environment.NewLine + Format.Code(executeResult.Exception.ToString()));
        }

        await context.Interaction.RespondAsync(embed: embed.Build());
    }

    private Task Log(LogMessage log)
    {
        logger.Log(log.Severity.ToLogLevel(), log.Exception, "{message}", log.Message);
        return Task.CompletedTask;
    }
}