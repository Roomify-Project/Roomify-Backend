using Microsoft.AspNetCore.Http;
using Roomify.GP.Core.Entities.AI;
using Roomify.GP.Core.Entities.AI.RoomImage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Service.Contract
{
    public interface IRoomImageService
    {
        Task<string> SaveImageAsync(IFormFile image);  // To Save/Send Image To The Server
        Task<RoomImage> GenerateRoomImageAsync(Guid userId, IFormFile imagePath, string descriptionText, RoomStyle roomStyle, RoomType roomType);  // For Creating the Image With It's Prompt and Sending It To The AI

        Task<AIResultHistory> SaveAIResultHistoryAsync(string imageUrl, Guid userId);  // For Saving The AIResultHistory
        Task<SavedDesign> SaveUserDesignAsync(string imageUrl, Guid userId);  // For Adding The Saved Designs
    }
}
