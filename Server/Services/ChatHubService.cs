using Microsoft.EntityFrameworkCore;
using Oqtane.ChatHubs.Repository;
using Oqtane.Modules;
using Oqtane.Shared.Enums;
using Oqtane.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Oqtane.ChatHubs.Services
{
    public class ChatHubService : IChatHubService, IService
    {

        private readonly IChatHubRepository chatHubRepository;

        public ChatHubService(
            IChatHubRepository chatHubRepository
            )
        {
            this.chatHubRepository = chatHubRepository;
        }

        public async Task<ChatHubRoom> CreateChatHubRoomClientModelAsync(ChatHubRoom room)
        {
            List<ChatHubMessage> lastMessages = new List<ChatHubMessage>();
            if(false && room.OneVsOne())
            {
                lastMessages = await this.chatHubRepository.GetChatHubMessages(room.Id).Take(10).ToListAsync();
                lastMessages = lastMessages != null && lastMessages.Any() ? lastMessages.Select(item => this.CreateChatHubMessageClientModel(item)).ToList() : new List<ChatHubMessage>();
            }

            List<ChatHubUser> onlineUsers = await this.chatHubRepository.GetChatHubUsersByRoom(room).Online().ToListAsync();
            onlineUsers = onlineUsers != null && onlineUsers.Any() ? onlineUsers = onlineUsers.Select(item => this.CreateChatHubUserClientModel(item)).ToList() : new List<ChatHubUser>();

            IQueryable<ChatHubModerator> moderatorsQuery = this.chatHubRepository.GetChatHubModerators(room);
            IList<ChatHubModerator> moderatorsList = await moderatorsQuery.ToListAsync();

            ChatHubUser creator = await this.chatHubRepository.GetUserByIdAsync(room.CreatorId);

            return new ChatHubRoom()
            {
                Id = room.Id,
                ModuleId = room.ModuleId,
                Title = room.Title,
                Content = room.Content,
                ImageUrl = room.ImageUrl,
                Type = room.Type,
                Status = room.Status,
                OneVsOneId = room.OneVsOneId,
                CreatorId = room.CreatorId,
                Creator = creator,
                Messages = lastMessages,
                Users = onlineUsers,
                Moderators = moderatorsList,
                CreatedOn = room.CreatedOn,
                CreatedBy = room.CreatedBy,
                ModifiedBy = room.ModifiedBy,
                ModifiedOn = room.ModifiedOn
            };
        }

        public ChatHubUser CreateChatHubUserClientModel(ChatHubUser user)
        {
            List<ChatHubConnection> activeConnections = this.chatHubRepository.GetConnectionsByUserId(user.UserId).Active().ToList();
            List<ChatHubConnection> activeConnectionsClientModels = activeConnections != null && !activeConnections.Any() ? new List<ChatHubConnection>() : activeConnections.Select(item => CreateChatHubConnectionClientModel(item)).ToList();

            ChatHubSetting chatHubSettings = this.chatHubRepository.GetChatHubSetting(user.UserId);
            ChatHubSetting chatHubSettingsClientModel = chatHubSettings != null ? this.CreateChatHubSettingClientModel(chatHubSettings) : null;

            return new ChatHubUser()
            {
                UserId = user.UserId,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Connections = activeConnectionsClientModels,
                Settings = chatHubSettingsClientModel,
                UserlistItemCollapsed = user.UserlistItemCollapsed,
                CreatedOn = user.CreatedOn,
                CreatedBy = user.CreatedBy,
                ModifiedOn = user.ModifiedOn,
                ModifiedBy = user.ModifiedBy
            };
        }

        public ChatHubMessage CreateChatHubMessageClientModel(ChatHubMessage message)
        {
            List<ChatHubPhoto> photos = message.Photos != null && message.Photos.Any() ? message.Photos.Select(item => CreateChatHubPhotoClientModel(item)).ToList() : null;
            ChatHubUser user = message.User != null ? this.CreateChatHubUserClientModel(message.User) : null;

            return new ChatHubMessage()
            {
                Id = message.Id,
                ChatHubRoomId = message.ChatHubRoomId,
                ChatHubUserId = message.ChatHubUserId,
                User = user,
                Content = message.Content,
                Type = message.Type,
                Photos = photos,
                CommandMetaDatas = message.CommandMetaDatas,
                CreatedOn = message.CreatedOn,
                CreatedBy = message.CreatedBy,
                ModifiedOn = message.ModifiedOn,
                ModifiedBy = message.ModifiedBy
            };
        }

        public ChatHubConnection CreateChatHubConnectionClientModel(ChatHubConnection connection)
        {
            return new ChatHubConnection()
            {
                ChatHubUserId = connection.ChatHubUserId,
                ConnectionId = this.MakeStringAnonymous(connection.ConnectionId, 7, '*'),
                Status = connection.Status,
                User = connection.User,
                CreatedOn = connection.CreatedOn,
                CreatedBy = connection.CreatedBy,
                ModifiedOn = connection.ModifiedOn,
                ModifiedBy = connection.ModifiedBy
            };
        }

        public ChatHubPhoto CreateChatHubPhotoClientModel(ChatHubPhoto photo)
        {
            return new ChatHubPhoto()
            {
                ChatHubMessageId = photo.ChatHubMessageId,
                Source = photo.Source,
                Thumb = photo.Thumb,
                Caption = photo.Caption,
                Size = photo.Size,
                Width = photo.Width,
                Height = photo.Height,
                CreatedOn = photo.CreatedOn,
                CreatedBy = photo.CreatedBy,
                ModifiedOn = photo.ModifiedOn,
                ModifiedBy = photo.ModifiedBy
            };
        }

        public ChatHubSetting CreateChatHubSettingClientModel(ChatHubSetting settings)
        {
            return new ChatHubSetting()
            {
                UsernameColor = settings.UsernameColor,
                MessageColor = settings.MessageColor,
                CreatedOn = settings.CreatedOn,
                CreatedBy = settings.CreatedBy,
                ModifiedOn = settings.ModifiedOn,
                ModifiedBy = settings.ModifiedBy
            };
        }

        public ChatHubModerator CreateChatHubModeratorClientModel(ChatHubModerator moderator)
        {
            return new ChatHubModerator()
            {
                Id = moderator.Id,
                ModeratorDisplayName = moderator.ModeratorDisplayName,
                ChatHubUserId = moderator.ChatHubUserId,
            };
        }

        public void IgnoreUser(ChatHubUser callerUser, ChatHubUser targetUser)
        {
            ChatHubIgnore chatHubIgnore = null;
            var users = this.chatHubRepository.GetIgnoredUsers(targetUser);
            chatHubIgnore = users.Where(u => u.ChatHubUserId == targetUser.UserId).FirstOrDefault();

            if (chatHubIgnore != null)
            {
                chatHubIgnore.ModifiedOn = DateTime.Now;
                this.chatHubRepository.UpdateChatHubIgnore(chatHubIgnore);
            }
            else
            {
                chatHubIgnore = new ChatHubIgnore()
                {
                    ChatHubUserId = callerUser.UserId,
                    ChatHubIgnoredUserId = targetUser.UserId,
                    User = callerUser
                };

                this.chatHubRepository.AddChatHubIgnore(chatHubIgnore);
            }
        }

        public List<string> GetAllExceptConnectionIds(ChatHubUser user)
        {
            var list = new List<ChatHubUser>();

            var ignoredUsers = this.chatHubRepository.GetIgnoredApplicationUsers(user).Where(x => x.Connections.Any(c => c.Status == Enum.GetName(typeof(ChatHubConnectionStatus), ChatHubConnectionStatus.Active))).ToList();
            var ignoredByUsers = this.chatHubRepository.GetIgnoredByApplicationUsers(user).Where(x => x.Connections.Any(c => c.Status == Enum.GetName(typeof(ChatHubConnectionStatus), ChatHubConnectionStatus.Active))).ToList();

            list.AddRange(ignoredUsers);
            list.AddRange(ignoredByUsers);

            List<string> connectionsIds = new List<string>();

            foreach (var item in list)
            {
                foreach (var connection in item.Connections.Active())
                {
                    connectionsIds.Add(connection.ConnectionId);
                }
            }

            return connectionsIds;
        }

        public ChatHubRoom GetOneVsOneRoom(ChatHubUser callerUser, ChatHubUser targetUser, int moduleId)
        {
            if (callerUser != null && targetUser != null)
            {
                var oneVsOneRoom = this.chatHubRepository.GetChatHubRoomOneVsOne(this.CreateOneVsOneId(callerUser, targetUser));
                if(oneVsOneRoom != null)
                {
                    return oneVsOneRoom;
                }

                ChatHubRoom chatHubRoom = new ChatHubRoom()
                {
                    ModuleId = moduleId,
                    Title = string.Format("{0} vs {1}", callerUser.DisplayName, targetUser.DisplayName),
                    Content = "One Vs One",
                    Type = ChatHubRoomType.OneVsOne.ToString(),
                    Status = ChatHubRoomStatus.Active.ToString(),
                    ImageUrl = string.Empty,
                    OneVsOneId = this.CreateOneVsOneId(callerUser, targetUser),
                    CreatorId = callerUser.UserId
                };
                return this.chatHubRepository.AddChatHubRoom(chatHubRoom);
            }

            return null;
        }
        public string CreateOneVsOneId(ChatHubUser user1, ChatHubUser user2)
        {
            var list = new List<string>();
            list.Add(user1.UserId.ToString());
            list.Add(user2.UserId.ToString());
            list = list.OrderBy(item => item).ToList();
            string roomId = string.Concat(list.First(), "|", list.Last());

            return roomId;
        }
        public bool IsValidOneVsOneConnection(ChatHubRoom room, ChatHubUser caller)
        {
            return room.OneVsOneId.Split('|').OrderBy(item => item).Any(item => item == caller.UserId.ToString());
        }
        public bool IsValidPrivateConnection(ChatHubRoom room, ChatHubUser caller)
        {
            var whitelistuser = this.chatHubRepository.GetChatHubWhitelistUser(caller.UserId);
            var room_whitelistuser = this.chatHubRepository.GetChatHubRoomChatHubWhitelistUser(room.Id, whitelistuser.Id);

            if(room_whitelistuser != null || caller.UserId == room.CreatorId)
            {
                return true;
            }

            return false;
        }
        public string MakeStringAnonymous(string value, int tolerance, char symbol = '*')
        {
            if (tolerance >= value.Length)
            {
                return string.Empty;
            }

            var newValue = value.Substring(0, value.Length - tolerance);
            for (var i = 0; i <= tolerance; i++)
            {
                newValue += symbol;
            }

            return newValue;
        }

    }
}
