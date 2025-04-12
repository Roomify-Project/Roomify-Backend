using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.DTOs.ChatModel
{
    public class ChatModel
    {
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string Message { get; set; }

    }
}
