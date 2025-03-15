using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace ContractsBot.Features.ContractsRanking.Preconditions;

public class IsHumanUserAttribute : ParameterPreconditionAttribute
{
    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, IParameterInfo parameterInfo, object value, IServiceProvider services)
    {
        if (value is not SocketUser socketUser)
        {
            return Task.FromResult(PreconditionResult.FromError("Wartość parametru nie jest użytkownikiem"));
        }

        if (socketUser.IsBot)
        {
            return Task.FromResult(PreconditionResult.FromError("Wybrany użytkownik nie może być botem"));
        }

        return Task.FromResult(PreconditionResult.FromSuccess());
    }
}
