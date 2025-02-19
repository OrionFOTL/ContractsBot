using MafiaContractsBot.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Plotly.NET;
using Plotly.NET.CSharp;
using Plotly.NET.ImageExport;
using Plotly.NET.LayoutObjects;
using Chart = Plotly.NET.CSharp.Chart;

namespace MafiaContractsBot.Features.ContractsRanking;

public class RankingService(DatabaseContext context)
{
    public async Task<int> GetRank(ulong userId)
    {
        var topPoints = await context.ContractUsers
            .Select(user => new
            {
                UserId = user.Id,
                TotalPoints = user.CompletedContracts.Sum(x => x.Points)
            })
            .OrderByDescending(x => x.TotalPoints)
            .ToListAsync();

        return topPoints.FindIndex(x => x.UserId == userId) + 1;
    }

    public async Task<TemporaryFile> GetRankingChart()
    {
        var topUsers = await context.ContractUsers
            .Include(u => u.CompletedContracts)
                .ThenInclude(cc => cc.Contract)
            .OrderByDescending(u => u.CompletedContracts.Sum(cc => cc.Points))
            .Take(10)
            .ToListAsync();

        var uniqueContracts = topUsers.SelectMany(u => u.CompletedContracts).Select(cc => cc.Contract).Distinct();

        // First get contracts grouped by their titles (Patapon, Ender Lilies, Postal 2)
        var stackedColumns = uniqueContracts
            .Select(contract => Chart
                .StackedColumn<int, string, string>(
                    values: topUsers // For each user, calculate their points for this specific contract
                        .Select(user => user.CompletedContracts
                            .FirstOrDefault(cc => cc.Contract.ThreadId == contract.ThreadId)
                            ?.Points ?? 0)
                        .ToList(),
                    Keys: topUsers.Select(u => u.Name).ToList(),
                    Name: contract.Title))
            .ToList();

        var chart = Chart
            .Combine(stackedColumns)
            .WithTitle("Ranking kontraktów")
            .WithLegendStyle(
                Y: -0.1,
                Orientation: StyleParam.Orientation.Horizontal,
                YAnchor: StyleParam.YAnchorPosition.Top,
                Font: Font.init(Size: 12))
            .WithLayout(
                Layout.init<string>(
                    Margin: Margin.init<int, int, int, int, int, bool>(Left: 15, Right: 15, Top: 70, Bottom: 00, Pad: 0, Autoexpand: true),
                    Font: Font.init(Size: 20)));

        var chartFilename = Guid.NewGuid().ToString();

        await chart.SavePNGAsync(chartFilename, Width: 1000, Height: 625);

        return new TemporaryFile(chartFilename + ".png");
    }
}
