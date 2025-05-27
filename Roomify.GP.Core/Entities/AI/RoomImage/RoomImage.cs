using Roomify.GP.Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Entities.AI.RoomImage
{
    public class RoomImage
    {
        public Guid Id { get; set; }
        public string ImagePath { get; set; }
        public DateTime CreatedDate { get; set; }


        public virtual Prompt Prompt { get; set; }

        // Navigation Properties
        public Guid ApplicationUserId { get; set; }    // FK
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}