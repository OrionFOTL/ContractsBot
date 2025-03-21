using ContractsBot.Configuration;
using ContractsBot.Extensions;
using ContractsBot.Features.ContractsRanking;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Options;

namespace ContractsBot;

internal class BotWorker(
    ILogger<BotWorker> logger,
    IOptions<DiscordClientOptions> discordClientOptions,
    IOptions<ServerOptions> serverOptions,
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

        var token = discordClientOptions.Value.Token;
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
        foreach (var id in serverOptions.Value.GuildIds)
        {
            var registeredCommands = await interactionService.RegisterCommandsToGuildAsync(id);

            ContractsRankingCommands.GuildToRankingCommandId.Add(id, registeredCommands.First(c => c.Name == "ranking").Id);
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
            .WithTitle("Błąd")
            .WithColor(Color.Red)
            .WithFooter("Orion")
            .WithDescription(result.ErrorReason);

        if (result is PreconditionGroupResult preconditionGroupResult)
        {
            embed = embed.WithDescription(preconditionGroupResult.Results.First(r => !r.IsSuccess).ErrorReason);
        }

        if (result is ExecuteResult { Error: InteractionCommandError.Exception, Exception: not null } executeResult)
        {
            embed = executeResult.Exception.InnerException is ContractsDomainException contractsException
                ? embed.WithDescription(contractsException.Message)
                : embed.WithDescription(
                    executeResult.Exception.Message + Environment.NewLine +
                    Format.Code(executeResult.Exception.ToString()) + Environment.NewLine +
                    $"Taguję {MentionUtils.MentionUser(194116215403184128)}");
        }

        await context.Interaction.RespondAsync(embed: embed.Build());
    }

    private Task Log(LogMessage log)
    {
        logger.Log(log.Severity.ToLogLevel(), log.Exception, "{message}", log.Message);
        return Task.CompletedTask;
    }
}