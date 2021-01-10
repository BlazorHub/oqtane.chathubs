﻿using System.Linq;
using System.Threading.Tasks;
using Oqtane.Models;
using Oqtane.Shared.Models;

namespace Oqtane.ChatHubs.Repository
{
    public interface IChatHubRepository
    {

        #region GET

        IQueryable<ChatHubRoom> GetChatHubRooms();
        IQueryable<ChatHubRoom> GetChatHubRoomsByModuleId(int ModuleId);
        IQueryable<ChatHubRoom> GetChatHubRoomsByUser(ChatHubUser user);
        IQueryable<ChatHubUser> GetChatHubUsersByRoom(ChatHubRoom room);
        ChatHubRoom GetChatHubRoom(int ChatHubRoomId);
        ChatHubRoom GetChatHubRoomOneVsOne(string OneVsOneId);
        IQueryable<ChatHubMessage> GetChatHubMessages(int roomId);
        ChatHubMessage GetChatHubMessage(int ChatHubMessageId);
        IQueryable<ChatHubUser> GetOnlineUsers();
        IQueryable<ChatHubUser> GetOnlineUsers(int roomId);
        IQueryable<ChatHubConnection> GetConnectionsByUserId(int userId);
        Task<ChatHubConnection> GetConnectionByConnectionId(string connectionId);
        ChatHubRoomChatHubUser GetChatHubRoomChatHubUser(int chatHubRoomId, int chatHubUserId);
        IQueryable<ChatHubIgnore> GetIgnoredUsers(ChatHubUser user);
        IQueryable<ChatHubUser> GetIgnoredApplicationUsers(ChatHubUser user);
        IQueryable<ChatHubUser> GetIgnoredByApplicationUsers(ChatHubUser user);
        IQueryable<ChatHubIgnore> GetIgnoredByUsers(ChatHubUser user);
        ChatHubSetting GetChatHubSetting(int ChatHubSettingId);
        ChatHubSetting GetChatHubSettingByUser(ChatHubUser user);
        Task<ChatHubUser> GetUserByIdAsync(int id);
        Task<ChatHubUser> GetUserByUserNameAsync(string username);
        Task<ChatHubUser> GetUserByDisplayName(string displayName);
        ChatHubModerator GetChatHubModerator(int ChatHubUserId);
        IQueryable<ChatHubModerator> GetChatHubModerators(ChatHubRoom ChatHubRoom);
        ChatHubRoomChatHubModerator GetChatHubRoomChatHubModerator(int chatHubRoomId, int chatHubModeratorId);
        ChatHubWhitelistUser GetChatHubWhitelistUser(int ChatHubUserId);
        IQueryable<ChatHubWhitelistUser> GetChatHubWhitelistUsers(ChatHubRoom ChatHubRoom);
        ChatHubRoomChatHubWhitelistUser GetChatHubRoomChatHubWhitelistUser(int chatHubRoomId, int chatHubWhitelistUserId);

        #endregion

        #region ADD

        ChatHubRoom AddChatHubRoom(ChatHubRoom ChatHubRoom);
        ChatHubMessage AddChatHubMessage(ChatHubMessage ChatHubMessage);
        ChatHubConnection AddChatHubConnection(ChatHubConnection ChatHubConnection);
        ChatHubUser AddChatHubUser(ChatHubUser ChatHubUser);
        ChatHubRoomChatHubUser AddChatHubRoomChatHubUser(ChatHubRoomChatHubUser ChatHubRoomChatHubUser);
        ChatHubPhoto AddChatHubPhoto(ChatHubPhoto ChatHubPhoto);
        ChatHubIgnore AddChatHubIgnore(ChatHubIgnore chatHubIgnore);
        ChatHubSetting AddChatHubSetting(ChatHubSetting ChatHubSetting);
        ChatHubModerator AddChatHubModerator(ChatHubModerator ChatHubModerator);
        ChatHubRoomChatHubModerator AddChatHubRoomChatHubModerator(ChatHubRoomChatHubModerator ChatHubRoomChatHubModerator);
        ChatHubWhitelistUser AddChatHubWhitelistUser(ChatHubUser targetUser);
        ChatHubRoomChatHubWhitelistUser AddChatHubRoomChatHubWhitelistUser(ChatHubRoomChatHubWhitelistUser ChatHubRoomChatHubWhitelistUser);

        #endregion

        #region DELETE

        void DeleteChatHubUser(int UserId);
        void DeleteChatHubRoom(int ChatHubRoomId, int ModuleId);
        void DeleteChatHubRooms(int userId, int ModuleId);
        void DeleteChatHubMessage(int ChatHubMessageId, int ChatHubRoomId);
        void DeleteChatHubMessages(int userId);
        void DeleteChatHubConnection(int ChatHubConnectionId, int ChatHubUserId);
        void DeleteChatHubConnections(int userId);
        void DeleteChatHubRoomChatHubUser(int ChatHubRoomId, int ChatHubUserId);
        void DeleteChatHubIgnore(ChatHubIgnore chatHubIgnore);
        void DeleteChatHubModerator(int ModeratorId);
        void DeleteChatHubRoomChatHubModerator(int ChatHubRoomId, int ChatHubModeratorId);
        void DeleteChatHubWhitelistUser(int WhitelistUserId);
        void DeleteChatHubRoomChatHubWhitelistUser(int ChatHubRoomId, int ChatHubWhitelistUserId);

        #endregion

        #region UPDATE

        Task UpdateUserAsync(User User);
        ChatHubRoom UpdateChatHubRoom(ChatHubRoom ChatHubRoom);
        ChatHubMessage UpdateChatHubMessage(ChatHubMessage ChatHubMessage);
        ChatHubConnection UpdateChatHubConnection(ChatHubConnection ChatHubConnection);
        ChatHubIgnore UpdateChatHubIgnore(ChatHubIgnore chatHubIgnore);
        ChatHubSetting UpdateChatHubSetting(ChatHubSetting ChatHubSetting);

        #endregion

    }
}
