using ContractsBot;
using ContractsBot.Configuration;
using ContractsBot.Extensions;
using ContractsBot.Features.ContractsRanking;
using ContractsBot.Infrastructure;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

var discordSocketConfig = new DiscordSocketConfig()
{
    GatewayIntents = GatewayIntents.AllUnprivileged
        & ~GatewayIntents.GuildScheduledEvents
        & ~GatewayIntents.GuildInvites,
};
var discordSocketClient = new DiscordSocketClient(discordSocketConfig);
var interactionService = new InteractionService(discordSocketClient);

builder.Services.AddSerilog(config => config.ReadFrom.Configuration(builder.Configuration));
builder.Services.AddAndValidateOptions<DiscordClientOptions>(DiscordClientOptions.SectionName);
builder.Services.AddAndValidateOptions<ServerOptions>(ServerOptions.SectionName);
builder.Services.AddAndValidateOptions<ChartOptions>(ChartOptions.SectionName);
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
