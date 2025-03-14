using ContractsBot.Configuration;
using ContractsBot.Extensions;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MafiaContractsBot;
using MafiaContractsBot.Features.ContractsRanking;
using MafiaContractsBot.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

var discordSocketConfig = new DiscordSocketConfig()
{
    GatewayIntents = GatewayIntents.AllUnprivileged
        & ~GatewayIntents.GuildScheduledEvents
        & ~GatewayIntents.GuildInvites,
};
var discordSocketClient = new DiscordSocketClient(discordSocketConfig);
var interactionService = new InteractionService(discordSocketClient);

builder.Services.AddAndValidateOptions<DiscordClientOptions>(DiscordClientOptions.SectionName);
builder.Services.AddAndValidateOptions<ServerOptions>(ServerOptions.SectionName);
builder.Services.AddSingleton(discordSocketClient);
builder.Services.AddSingleton(interactionService);
builder.Services.AddDbContext<DatabaseContext>(o => o.UseSqlite("Data Source=contracts.db"));
builder.Services.AddScoped<RankingService>();
builder.Services.AddHostedService<BotWorker>();
builder.Services.AddSystemd();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    await dbContext.Database.MigrateAsync();
}

host.Run();
