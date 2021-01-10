using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.ChatHubs.Shared.Enums;
using Oqtane.Models;

namespace Oqtane.Shared.Models
{

    public class ChatHubUser : User
    {

        public ChatHubRoomLevelType RoomLevel { get; set; }

        [NotMapped]
        public bool UserlistItemCollapsed { get; set; }

        [NotMapped]
        public virtual IList<ChatHubRoomChatHubUser> UserRooms { get; set; }

        [NotMapped]
        public virtual IList<ChatHubConnection> Connections { get; set; }

        [NotMapped]
        public virtual ChatHubSetting Settings { get; set; }

        [NotMapped]
        public virtual ChatHubCam Cam { get; set; }

        [NotMapped]
        public virtual ICollection<ChatHubIgnore> Ignores { get; set; }

    }
}