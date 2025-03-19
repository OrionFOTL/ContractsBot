using ContractsBot.Configuration;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Options;

namespace ContractsBot.Features.ContractsRanking.Preconditions;

public class IsInContractManagerRoleAttribute : PreconditionAttribute
{
    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        var serverOptions = services.GetRequiredService<IOptions<ServerOptions>>();
        var adminRoleIds = serverOptions.Value.ContractManagerRoleIds;

        if (context.User is not IGuildUser guildUser)
        {
            return Task.FromResult(PreconditionResult.FromError("Nie jesteś użytkownikiem serwera"));
        }

        if (!guildUser.RoleIds.Any(roleId => adminRoleIds.Contains(roleId)))
        {
            return Task.FromResult(PreconditionResult.FromError("Nie masz uprawnień do wykonania tej komendy"));
        }

        return Task.FromResult(PreconditionResult.FromSuccess());
    }
}
