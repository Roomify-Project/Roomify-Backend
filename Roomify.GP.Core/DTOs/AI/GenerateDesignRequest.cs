using Microsoft.AspNetCore.Http;
using Roomify.GP.Core.Entities.AI.RoomImage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.DTOs.GenerateDesign
{
    public class GenerateDesignRequest
    {
        public IFormFile Image { get; set; }
        public string DescriptionText { get; set; }
        public RoomStyle RoomStyle { get; set; }
        public RoomType RoomType { get; set; }  
        public Guid UserId { get; set; }
        public bool SaveToHistory { get; set; } = true;
    }
}
