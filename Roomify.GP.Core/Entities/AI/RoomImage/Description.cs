using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Entities.AI.RoomImage
{
    public class Description
    {
        public Guid Id { get; set; }
        public string DescriptionText { get; set; }
        public RoomStyle RoomStyle { get; set; }  // Enum
        public RoomType RoomType { get; set; }    // Enum

        // FK
        public Guid RoomImageId { get; set; } 
        public virtual RoomImage RoomImage { get; set; }

    }
}
