using Oqtane.Shared.Models;
using System.Collections.Generic;
using System.Linq;

namespace Oqtane.Shared.Extensions
{
    public static class ChatHubServiceExtensionMethods
    {

        public static void AddRoom(this List<ChatHubRoom> rooms, ChatHubRoom room)
        {
            if (!rooms.Any(x => x.Id == room.Id))
            {
                rooms.Add(room);
            }
        }

        public static void RemoveRoom(this List<ChatHubRoom> rooms, ChatHubRoom room)
        {
            var chatRoom = rooms.First(x => x.Id == room.Id);
            if (chatRoom != null)
            {
                rooms.Remove(chatRoom);
            }
        }

        public static void AddMessage(this ChatHubRoom room, ChatHubMessage message)
        {
            if (!room.Messages.Any(x => x.Id == message.Id))
            {
                room.Messages.Add(message);
            }
        }

        public static void AddUser(this List<ChatHubRoom> rooms, ChatHubUser user, string roomId)
        {
            var room = rooms.FirstOrDefault(x => x.Id.ToString() == roomId);
            if (room != null && !room.Users.Any(x => x.UserId == user.UserId))
            {
                room.Users.Add(user);
            }
        }

        public static void RemoveUser(this List<ChatHubRoom> rooms, ChatHubUser user, string roomId)
        {
            var room = rooms.FirstOrDefault(x => x.Id.ToString() == roomId);
            if (room != null)
            {
                var userItem = room.Users.FirstOrDefault(x => x.UserId == user.UserId);
                if (userItem != null)
                {
                    room.Users.Remove(userItem);
                }
            }
        }
    }

}
