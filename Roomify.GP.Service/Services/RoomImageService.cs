using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Roomify.GP.Core.Entities.AI;
using Roomify.GP.Core.Entities.AI.RoomImage;
using Roomify.GP.Core.Repositories.Contract;
using Roomify.GP.Core.Service.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Roomify.GP.Service.Services
{
    public class RoomImageService : IRoomImageService
    {
        private readonly IAIResultHistoryRepository _aiResultHistoryRepository;
        private readonly ISavedDesignRepository _savedDesignRepository;
        private readonly IPromptRepository _promptRepository;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<RoomImageService> _logger;

        public RoomImageService(
            IAIResultHistoryRepository aiResultHistoryRepository,
            ISavedDesignRepository savedDesignRepository,
            IPromptRepository promptRepository,
            ICloudinaryService cloudinaryService,
            ILogger<RoomImageService> logger)
        {
            _aiResultHistoryRepository = aiResultHistoryRepository;
            _savedDesignRepository = savedDesignRepository;
            _promptRepository = promptRepository;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        public async Task<string> SaveImageAsync(IFormFile image)
        {
            // Upload to Cloudinary
            return await _cloudinaryService.UploadImageAsync(image);
        }

        // Save a generated AI result to history
        public async Task<AIResultHistory> SaveAIResultHistoryAsync(string generatedImageUrl, Guid userId)
        {
            // Create AIResultHistory directly with the URL
            var aiResultHistory = new AIResultHistory
            {
                Id = Guid.NewGuid(),
                GeneratedImageUrl = generatedImageUrl,
                CreatedAt = DateTime.UtcNow,
                ApplicationUserId = userId
            };

            // Add to database
            await _aiResultHistoryRepository.AddAsync(aiResultHistory);
            return aiResultHistory;
        }

        // Save both the image result and its associated prompt information
        public async Task<AIResultHistory> SaveGeneratedDesignWithPromptAsync(
            string generatedImageUrl,
            Guid userId,
            string descriptionText,
            RoomStyle roomStyle,
            RoomType roomType)
        {
            // First save the image result to history
            var aiResultHistory = await SaveAIResultHistoryAsync(generatedImageUrl, userId);

            // Then create and save the associated prompt
            var prompt = new Prompt
            {
                Id = Guid.NewGuid(),
                DescriptionText = descriptionText,
                RoomStyle = roomStyle,
                RoomType = roomType,
                AIResultHistoryId = aiResultHistory.Id  // Link to the history entry
            };

            await _promptRepository.AddAsync(prompt);

            return aiResultHistory;
        }

        // Enhanced SaveUserDesignAsync that ensures permanent storage
        public async Task<SavedDesign> SaveUserDesignAsync(string imageUrl, Guid userId)
        {
            try
            {
                // Download the image from Replicate (or any URL)
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromMinutes(2); // Set timeout

                var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);

                // Create a memory stream for the image data
                using var stream = new MemoryStream(imageBytes);

                // Reset stream position to beginning
                stream.Position = 0;

                // Create a custom IFormFile implementation
                var tempFile = new CustomFormFile(stream, "saved-design.jpg", "image/jpeg");

                // Upload to Cloudinary for permanent storage
                var permanentUrl = await _cloudinaryService.UploadImageAsync(tempFile);

                if (string.IsNullOrEmpty(permanentUrl))
                    throw new InvalidOperationException("Failed to upload image to Cloudinary");

                // Save to database with the permanent Cloudinary URL
                var savedDesign = new SavedDesign
                {
                    Id = Guid.NewGuid(),
                    GeneratedImageUrl = permanentUrl, // Now using the permanent Cloudinary URL
                    SavedAt = DateTime.UtcNow,
                    ApplicationUserId = userId
                };

                await _savedDesignRepository.AddAsync(savedDesign);
                _logger.LogInformation($"Design saved permanently for user {userId} at {permanentUrl}");

                return savedDesign;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Network error downloading image from {imageUrl}");
                throw new InvalidOperationException($"Failed to download image: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, $"Timeout downloading image from {imageUrl}");
                throw new InvalidOperationException("Image download timed out", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving design permanently");
                throw;
            }
        }

        // Get user's history of generated designs
        public async Task<List<AIResultHistory>> GetUserHistoryAsync(Guid userId)
        {
            return await _aiResultHistoryRepository.GetByUserIdAsync(userId);
        }

        // Get user's saved designs
        public async Task<List<SavedDesign>> GetUserSavedDesignsAsync(Guid userId)
        {
            return await _savedDesignRepository.GetByUserIdAsync(userId);
        }

        //Get user's saved designs with user information
        public async Task<List<SavedDesign>> GetUserSavedDesignsWithUserInfoAsync(Guid userId)
        {
            return await _savedDesignRepository.GetByUserIdWithUserInfoAsync(userId);
        }

        //Get all saved designs with user information
        public async Task<List<SavedDesign>> GetAllSavedDesignsWithUserInfoAsync()
        {
            return await _savedDesignRepository.GetAllWithUserInfoAsync();
        }

        //Get saved design by ID with user information
        public async Task<SavedDesign> GetSavedDesignByIdWithUserInfoAsync(Guid id)
        {
            return await _savedDesignRepository.GetByIdWithUserInfoAsync(id);
        }
    }

}