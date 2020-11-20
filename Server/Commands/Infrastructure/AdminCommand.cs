﻿using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Oqtane.Shared.Models;

namespace Oqtane.ChatHubs.Commands
{
    public abstract class AdminCommand : BaseCommand
    {
        public override async Task Execute(CommandServicesContext commandServicesContext, CommandCallerContext commandCallerContext, string[] args, ChatHubUser caller)
        {
            IdentityUser identityUser = await commandServicesContext.UserManager.FindByNameAsync(caller.Username);
            if(!commandServicesContext.ChatHub.Context.User.HasClaim(ClaimTypes.Role, Shared.Constants.AdminRole))
            {
                throw new HubException("You do not have any permissions to run this command");
            }

            await ExecuteAdminOperation(commandServicesContext, commandCallerContext, args, caller);
        }

        public abstract Task ExecuteAdminOperation(CommandServicesContext context, CommandCallerContext callerContext, string[] args, ChatHubUser caller);
    }
}
