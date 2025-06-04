using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.DTOs.ChatModel
{
    public class ChatPreviewDto
    {
        public string ChatWithUserId { get; set; }
        public string ChatWithName { get; set; }
        public string ChatWithImageUrl { get; set; }
        public string LastMessageContent { get; set; }
        public DateTime LastMessageTime { get; set; }
    }
}
