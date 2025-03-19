using System.ComponentModel.DataAnnotations;

namespace ContractsBot.Configuration;

internal class ServerOptions
{
    public const string SectionName = "Servers";

    [Required] public required ulong[] GuildIds { get; init; }

    public required ulong[] ContractManagerRoleIds { get; init; } = [];
}
