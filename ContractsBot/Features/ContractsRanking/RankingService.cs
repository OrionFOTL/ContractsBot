using ContractsBot.Infrastructure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.SkiaSharpView.VisualElements;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

namespace ContractsBot.Features.ContractsRanking;

public class RankingService(DatabaseContext context)
{
    private static SolidColorPaint BlackOpenSansPaint => new(new SKColor(30, 30, 30, 255))
    {
        SKTypeface = SKTypeface.FromFamilyName("Noto Sans JP")
    };

    public async Task<int> GetRank(ulong userId)
    {
        var rank = await context.ContractUsers
            .Where(user => user.Id == userId)
            .Select(user => new
            {
                Rank = context.ContractUsers
                    .Count(other => other.CompletedContracts.Sum(x => x.Points) >
                                    user.CompletedContracts.Sum(x => x.Points)) + 1
            })
            .FirstOrDefaultAsync();

        return rank?.Rank ?? -1;
    }

    public async Task<Stream> GetRankingChart()
    {
        var topUsers = await context.ContractUsers
            .Include(u => u.CompletedContracts)
                .ThenInclude(cc => cc.Contract)
            .OrderByDescending(u => u.CompletedContracts.Sum(cc => cc.Points))
            .Take(10)
            .ToListAsync();

        var uniqueContracts = topUsers.SelectMany(u => u.CompletedContracts).Select(cc => cc.Contract).Distinct();

        var series = uniqueContracts.Select(contract => new StackedColumnSeries<int>
        {
            Values = topUsers
                .Select(user => user
                    .CompletedContracts
                    .FirstOrDefault(cc => cc.Contract.ThreadId == contract.ThreadId)
                    ?.Points ?? 0)
                .ToList(),
            Name = contract.Title,
            Stroke = null,
            MaxBarWidth = 250,
            Padding = 10,
            DataLabelsPaint = new SolidColorPaint(SKColors.Black),
            DataLabelsSize = 14,
            DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Middle,
            DataLabelsFormatter = p => p.Coordinate.PrimaryValue != 0 ? p.Coordinate.PrimaryValue.ToString() : string.Empty,
        }).ToArray();

        var chart = new SKCartesianChart
        {
            Series = series,
            XAxes =
            [
                new Axis
                {
                    Labels = topUsers.Select(u => u.Name).ToArray(),
                    LabelsPaint = BlackOpenSansPaint,
                    LabelsRotation = -30,
                    Padding = new(1),
                }
            ],
            YAxes =
            [
                new Axis()
                {
                    MinLimit = 0,
                    LabelsPaint = BlackOpenSansPaint,
                }
            ],
            Title = new LabelVisual
            {
                Text = "Ranking kontraktów",
                TextSize = 20,
                Paint = BlackOpenSansPaint,
            },
            LegendPosition = LiveChartsCore.Measure.LegendPosition.Right,
            LegendTextPaint = new SolidColorPaint(SKColors.Black),
            LegendBackgroundPaint = new SolidColorPaint(SKColors.LightGray),
            LegendTextSize = 11,
        };

        return chart.GetImage().Encode().AsStream();
    }
}
