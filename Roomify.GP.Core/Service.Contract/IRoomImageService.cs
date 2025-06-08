using Microsoft.AspNetCore.Http;
using Roomify.GP.Core.Entities.AI.RoomImage;
using Roomify.GP.Core.Entities.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Service.Contract
{
    public interface IRoomImageService
    {
        // Upload an image to Cloudinary and return its URL
        Task<string> SaveImageAsync(IFormFile image);

        // Save a generated AI result to history
        Task<AIResultHistory> SaveAIResultHistoryAsync(string generatedImageUrl, Guid userId);

        // Save both the image result and its associated prompt
        Task<AIResultHistory> SaveGeneratedDesignWithPromptAsync(
            string generatedImageUrl,
            Guid userId,
            string descriptionText,
            RoomStyle roomStyle,
            RoomType roomType);

        // Save a design to user's saved designs
        Task<SavedDesign> SaveUserDesignAsync(string imageUrl, Guid userId);

        // Get user's history of generated designs
        Task<List<AIResultHistory>> GetUserHistoryAsync(Guid userId);

        // Get User saved designs with info 
        Task<List<SavedDesign>> GetUserSavedDesignsWithUserInfoAsync(Guid userId);

        // Gets all saved designs with user info
        Task<List<SavedDesign>> GetAllSavedDesignsWithUserInfoAsync();

        // Gets a specific saved design by ID with user info
        Task<SavedDesign> GetSavedDesignByIdWithUserInfoAsync(Guid id);
    }
}
