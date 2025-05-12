using System.ComponentModel.DataAnnotations;

namespace ContractsBot.Configuration;

public record ChartOptions
{
    public const string SectionName = "ChartOptions";

    [Required, Range(-360, 360)]
    public required int LabelsRotation { get; init; }
}
