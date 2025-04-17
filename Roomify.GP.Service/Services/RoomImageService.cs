using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Roomify.GP.Core.Entities.AI;
using Roomify.GP.Core.Entities.AI.RoomImage;
using Roomify.GP.Core.Repositories.Contract;
using Roomify.GP.Core.Service.Contract;
using Roomify.GP.Repository.Data.Contexts;
using Roomify.GP.Repository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Service.Services
{

    public class RoomImageService : IRoomImageService
    {
        private readonly IRoomImageRepository _roomImageRepository;
        private readonly IPromptRepository _promptRepository;
        private readonly IAIResultHistoryRepository _aiResultHistoryRepository;
        private readonly ISavedDesignRepository _savedDesignRepository;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly AppDbContext _context;

        public RoomImageService(IRoomImageRepository roomImageRepository, IPromptRepository promptRepository, IAIResultHistoryRepository aIResultHistoryRepository, ISavedDesignRepository savedDesignRepository, ICloudinaryService cloudinaryService, AppDbContext context)
        {
            _roomImageRepository = roomImageRepository;
            _promptRepository = promptRepository;
            _aiResultHistoryRepository = aIResultHistoryRepository;
            _savedDesignRepository = savedDesignRepository;
            _cloudinaryService = cloudinaryService;
            _context = context;
        }

        public async Task<string> SaveImageAsync(IFormFile image)
        {
            // Uploading to Cloudinary
            return await _cloudinaryService.UploadImageAsync(image);  
        }

        public async Task<RoomImage> GenerateRoomImageAsync(Guid userId, IFormFile imagePath, string descriptionText, RoomStyle roomStyle, RoomType roomType)
        {
            var UserId = _context.RoomImages.Where(R => R.ApplicationUserId.ToString() == userId.ToString()).ToListAsync();

            // رفع الصورة إلى Cloudinary بعد توليدها
            var imageUrl = await SaveImageAsync(imagePath);

            // حفظ الصورة في قاعدة البيانات
            var roomImage = new RoomImage
            {
                ImagePath = imageUrl,  // حفظ رابط الصورة من Cloudinary
                ApplicationUserId = userId,
                CreatedDate = DateTime.UtcNow
            };
            await _roomImageRepository.AddAsync(roomImage);

            // إنشاء الوصف وربطه بالصورة
            var prompt = new Prompt
            {
                DescriptionText = descriptionText,
                RoomStyle = roomStyle,
                RoomType = roomType,
                RoomImageId = roomImage.Id
            };
            await _promptRepository.AddAsync(prompt);

            return roomImage;
        }


        // Saving The AIResult History
        // Updating the method to accept a URL instead of an ID
        public async Task<AIResultHistory> SaveAIResultHistoryAsync(string generatedImageUrl, Guid userId)
        {
            // Create AIResultHistory directly with the URL
            var aiResultHistory = new AIResultHistory
            {
                generatedImageUrl = generatedImageUrl,
                CreatedAt = DateTime.UtcNow,
                ApplicationUserId = userId
            };

            // Add to database
            await _aiResultHistoryRepository.AddAsync(aiResultHistory);

            return aiResultHistory;
        }

        //Saving The User SavedDesigns Generated From AI
        public async Task<SavedDesign> SaveUserDesignAsync(string imageUrl, Guid userId)
        {
            var savedDesign = new SavedDesign
            {
                GeneratedImageUrl = imageUrl,
                SavedAt = DateTime.UtcNow,
                ApplicationUserId = userId
            };

            await _savedDesignRepository.AddAsync(savedDesign);
            return savedDesign;
        }

    }

}
