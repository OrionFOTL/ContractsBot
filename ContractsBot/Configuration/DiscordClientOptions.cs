using System.ComponentModel.DataAnnotations;

namespace ContractsBot.Configuration;

internal class DiscordClientOptions
{
    public const string SectionName = "DiscordClient";

    [Required] public required string Token { get; init; }
}
