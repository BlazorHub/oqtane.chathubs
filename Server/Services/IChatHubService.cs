﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Oqtane.Modules;
using Oqtane.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.ChatHubs.Services
{
    public interface IChatHubService
    {

        Task<ChatHubRoom> CreateChatHubRoomClientModelAsync(ChatHubRoom room);

        ChatHubUser CreateChatHubUserClientModel(ChatHubUser chatHubUser);

        ChatHubMessage CreateChatHubMessageClientModel(ChatHubMessage message);

        ChatHubPhoto CreateChatHubPhotoClientModel(ChatHubPhoto photo);

        ChatHubModerator CreateChatHubModeratorClientModel(ChatHubModerator moderator);

        void IgnoreUser(ChatHubUser guest, ChatHubUser targetUser);

        List<string> GetAllExceptConnectionIds(ChatHubUser user);

        ChatHubRoom GetOneVsOneRoom(ChatHubUser caller, ChatHubUser targetUser, int moduleId);

        string CreateOneVsOneId(ChatHubUser user1, ChatHubUser user2);

        bool IsValidOneVsOneConnection(ChatHubRoom room, ChatHubUser caller);

        string MakeStringAnonymous(string value, int tolerance, char symbol = '*');

    }
}
