using System;
using System.ComponentModel.DataAnnotations.Schema;
using Oqtane.Models;

namespace Oqtane.StreamHubs.Models
{
    public class ChatHub : IAuditable
    {
        public int ChatHubId { get; set; }
        public int ModuleId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
}
