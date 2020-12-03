using Oqtane.Shared.Models;
using System.Composition;
using System.Threading.Tasks;
using Oqtane.Shared;
using Oqtane.Shared.Enums;
using Microsoft.AspNetCore.SignalR;
using System;

namespace Oqtane.ChatHubs.Commands
{
    [Export("ICommand", typeof(ICommand))]
    [Command("ciaobella", "[username]", new string[] { Constants.AdminRole }, "Usage: /ciaobella")]
    public class CiaoBellaCommand : AdminCommand
    {
        public override async Task ExecuteAdminOperation(CommandServicesContext context, CommandCallerContext callerContext, string[] args, ChatHubUser caller)
        {

            if (args.Length == 0)
            {
                await context.ChatHub.SendClientNotification("No arguments found.", callerContext.RoomId, callerContext.ConnectionId, caller, ChatHubMessageType.System);
                return;
            }

            string targetUserName = args[0];

            ChatHubUser targetUser = await context.ChatHubRepository.GetUserByDisplayName(targetUserName);
            targetUser = targetUser == null ? await context.ChatHubRepository.GetUserByUserNameAsync(targetUserName) : targetUser;
            if (targetUser == null)
            {
                await context.ChatHub.SendClientNotification("No user found.", callerContext.RoomId, callerContext.ConnectionId, caller, ChatHubMessageType.System);
                return;
            }

            // TODO: delete user with all its relations
            throw new HubException("somhow is this not implemented yet but how why??", new NotImplementedException("??"));

            foreach(var connection in targetUser.Connections)
            {
                await context.ChatHub.Clients.Client(connection.ConnectionId).SendAsync("Disconnect", context.ChatHubService.CreateChatHubUserClientModel(targetUser));
            }

        }
    }
}