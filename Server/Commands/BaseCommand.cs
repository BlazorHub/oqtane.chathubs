﻿using Oqtane.Shared.Models;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace Oqtane.ChatHubs.Commands
{
    [Export("ICommand", typeof(object))]
    public abstract class BaseCommand : ICommand
    {
        async Task ICommand.Execute(CommandServicesContext commandServiceContext, CommandCallerContext commandCallerContext, string[] args)
        {
            ChatHubUser user = commandServiceContext.ChatHubRepository.GetUserByIdAsync(commandCallerContext.UserId).GetAwaiter().GetResult();
            await Execute(commandServiceContext, commandCallerContext, args, user);
        }

        public abstract Task Execute(CommandServicesContext context, CommandCallerContext callerContext, string[] args, ChatHubUser caller);
    }
}