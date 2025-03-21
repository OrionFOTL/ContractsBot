﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace ContractsBot.Features.ContractsRanking.Preconditions;

public class IsTheirOwnForumThreadAttribute : PreconditionAttribute
{
    public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        if (context.Channel is not SocketThreadChannel { ParentChannel: SocketForumChannel } forumThread)
        {
            return Task.FromResult(PreconditionResult.FromError("Tej komendy można używać tylko w wątku kontraktu"));
        }

        if (((IThreadChannel)forumThread).OwnerId != context.User.Id)
        {
            return Task.FromResult(PreconditionResult.FromError("Tylko autor kontraktu może używać tej komendy"));
        }

        return Task.FromResult(PreconditionResult.FromSuccess());
    }
}
