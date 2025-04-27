using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace ContractsBot.Features.ContractsRanking.Preconditions;

public class IsInForumThread : PreconditionAttribute
{
    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        if (context.Channel is not SocketThreadChannel { ParentChannel: SocketForumChannel } forumThread)
        {
            return Task.FromResult(PreconditionResult.FromError("Tej komendy można używać tylko w wątku kontraktu"));
        }

        return Task.FromResult(PreconditionResult.FromSuccess());
    }
}
