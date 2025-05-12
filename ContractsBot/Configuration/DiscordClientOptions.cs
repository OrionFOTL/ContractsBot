using System.ComponentModel.DataAnnotations;

namespace ContractsBot.Configuration;

internal record DiscordClientOptions
{
    public const string SectionName = "DiscordClient";

    [Required] public required string Token { get; init; }
}
