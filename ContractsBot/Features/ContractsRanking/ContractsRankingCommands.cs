using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using MafiaContractsBot.Features.ContractsRanking.Models;
using MafiaContractsBot.Features.ContractsRanking.Preconditions;
using MafiaContractsBot.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Plotly.NET;

namespace MafiaContractsBot.Features.ContractsRanking;

public class ContractsRankingCommands(DatabaseContext dbContext, RankingService rankingService) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("complete-contract", "Dodaj userowi punkty za ukończenie tego kontraktu")]
    [IsTheirOwnForumThread]
    public async Task CompleteContract(SocketUser user, int points)
    {
        var threadChannel = (SocketThreadChannel)Context.Channel;

        var contractUser = await dbContext.ContractUsers
            .Include(u => u.CompletedContracts)
                .ThenInclude(cc => cc.Contract)
            .FirstOrDefaultAsync(x => x.Id == user.Id)
            ?? dbContext.ContractUsers.Add(new ContractUser { Id = user.Id, Name = user.GlobalName }).Entity;

        var contract = await dbContext.Contracts.FirstOrDefaultAsync(x => x.ThreadId == threadChannel.Id)
            ?? dbContext.Contracts.Add(new Contract { ThreadId = threadChannel.Id, Title = threadChannel.Name }).Entity;

        contractUser.CompleteContract(contract, points);

        await dbContext.SaveChangesAsync();

        var userRank = await rankingService.GetRank(contractUser.Id);
        var medalEmoji = userRank switch
        {
            1 => "🥇",
            2 => "🥈",
            3 => "🥉",
            _ => ""
        };

        string text =
            $"""
            Dodano {points} punktów dla {user.Mention} za ukończenie kontraktu {Format.Bold(threadChannel.Name)}!

            Ma teraz w sumie {contractUser.CompletedContracts.Sum(x => x.Points)} punktów i jest na {medalEmoji}{userRank}. miejscu w serwerowym rankingu!

            {Format.Subtext($"Użyj {Format.Code("/ranking")} albo {Format.Code("/ranking-chart")} aby wyświetlić serwerową listę najlepszych~!")}
            """;

        var embed = new EmbedBuilder()
            .WithTitle("Ukończono kontrakt!")
            .WithDescription(text)
            .WithColor(Discord.Color.Green)
            .Build();

        await RespondAsync(embed: embed);

    }

    [SlashCommand("ranking", "Wyświetl serwerowy ranking kontraktów w formie listy i wykresu")]
    public async Task Ranking()
    {
        await DeferAsync();

        var ranking = await dbContext.ContractUsers
            .Include(u => u.CompletedContracts)
                .ThenInclude(cc => cc.Contract)
            .OrderByDescending(u => u.CompletedContracts.Sum(cc => cc.Points))
            .Take(3)
            .ToListAsync();

        var rankStrings =
            from user in ranking
            let position = ranking.IndexOf(user) + 1
            let totalPoints = user.CompletedContracts.Sum(cc => cc.Points)
            let medalEmoji = position switch
            {
                1 => "🥇",
                2 => "🥈",
                3 => "🥉",
                _ => string.Empty,
            }
            let userRankString = $"{position}. {medalEmoji} {MentionUtils.MentionUser(user.Id)} - {totalPoints} pkt"
            let userContracts = string.Join(Environment.NewLine, user.CompletedContracts.OrderByDescending(cc => cc.Points).Select(cc => $"  - {MentionUtils.MentionChannel(cc.Contract.ThreadId)} ({cc.Points} pkt)"))
            select userRankString + Environment.NewLine + userContracts;

        var rankingText = string.Join(Environment.NewLine, rankStrings);

        using var rankingChart = await rankingService.GetRankingChart();

        var embed = new EmbedBuilder()
            .WithTitle("Ranking kontraktów")
            .WithImageUrl($"attachment://{rankingChart.File.Name}")
            .WithThumbnailUrl("https://i.imgur.com/BEqMP5X.png")
            .WithDescription(rankingText)
            .WithColor(Discord.Color.Blue)
            .Build();

        await FollowupWithFileAsync(rankingChart.FileStream, rankingChart.File.Name, embed: embed, allowedMentions: AllowedMentions.None);
    }

    //[SlashCommand("ranking-chart", "Wyświetl serwerowy ranking konktraktów w formie samego wykresu")]
    //public async Task SaveRankingChart()
    //{
    //    await DeferAsync();

    //    using var rankingChart = await rankingService.GetRankingChart();

    //    await FollowupWithFileAsync(rankingChart.FileStream, rankingChart.File.Name);
    //}
}