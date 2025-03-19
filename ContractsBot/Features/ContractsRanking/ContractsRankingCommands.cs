using ContractsBot.Features.ContractsRanking.Models;
using ContractsBot.Features.ContractsRanking.Preconditions;
using ContractsBot.Infrastructure;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace ContractsBot.Features.ContractsRanking;

public class ContractsRankingCommands(DatabaseContext dbContext, RankingService rankingService) : InteractionModuleBase<SocketInteractionContext>
{
    public static Dictionary<ulong, ulong> GuildToRankingCommandId { get; } = [];

    [IsTheirOwnForumThread(Group = "Group")]
    [IsInContractManagerRole(Group = "Group")]
    [RequireOwner(Group = "Group")]
    [SlashCommand("set-points", "Dodaj/edytuj userowi punkty za ukończenie tego kontraktu")]
    public async Task SetPoints(
        [Summary("użytkownik", "Komu przyznać punkty"), IsHumanUser] SocketUser user,
        [Summary("punkty", "Liczba punktów za ukończenie kontraktu razem z opcjonalnymi celami")] int points)
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
            Przyznano {points} punktów dla {user.Mention} za ukończenie kontraktu {Format.Bold(threadChannel.Name)}!

            Ma teraz w sumie {contractUser.CompletedContracts.Sum(x => x.Points)} punktów i jest na {medalEmoji}{userRank}. miejscu w serwerowym rankingu!

            {Format.Subtext($"Użyj {(Context.Guild != null && GuildToRankingCommandId.TryGetValue(Context.Guild.Id, out var commandId) ? $"</ranking:{commandId}>" : Format.Code("/ranking"))} aby wyświetlić serwerową listę najlepszych~!")}
            """;

        var embed = new EmbedBuilder()
            .WithTitle("Ukończono kontrakt!")
            .WithDescription(text)
            .WithThumbnailUrl("https://i.imgur.com/BEqMP5X.png")
            .WithColor(Color.Green)
            .Build();

        await RespondAsync(embed: embed);

    }

    [SlashCommand("ranking", "Wyświetl serwerowy ranking kontraktów w formie listy i wykresu")]
    public async Task Ranking([Summary("top", "Ilu najlepszych użytkowników pokazać na liście")] int top = 3)
    {
        await DeferAsync();

        var ranking = await dbContext.ContractUsers
            .Include(u => u.CompletedContracts)
                .ThenInclude(cc => cc.Contract)
            .OrderByDescending(u => u.CompletedContracts.Sum(cc => cc.Points))
            .Take(top)
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
            .WithImageUrl($"attachment://ranking.png")
            .WithThumbnailUrl("https://i.imgur.com/BEqMP5X.png")
            .WithDescription(rankingText)
            .WithColor(Color.Blue)
            .WithFooter("Orion")
            .Build();

        await FollowupWithFileAsync(rankingChart, "ranking.png", embed: embed, allowedMentions: AllowedMentions.None);
    }

    //[SlashCommand("ranking-chart", "Wyświetl serwerowy ranking konktraktów w formie samego wykresu")]
    //public async Task SaveRankingChart()
    //{
    //    await DeferAsync();

    //    using var rankingChart = await rankingService.GetRankingChart();

    //    await FollowupWithFileAsync(rankingChart.FileStream, rankingChart.File.Name);
    //}
}