using Microsoft.AspNetCore.Http;
using Roomify.GP.Core.Entities.AI;
using Roomify.GP.Core.Entities.AI.RoomImage;
using Roomify.GP.Core.Repositories.Contract;
using Roomify.GP.Core.Service.Contract;
using Roomify.GP.Repository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Service.Services
{

    public class RoomImageService :IRoomImageService
    {
        private readonly IRoomImageRepository _roomImageRepository;
        private readonly IPromptRepository _promptRepository;
        private readonly IAIResultHistoryRepository _aiResultHistoryRepository;
        private readonly ISavedDesignRepository _savedDesignRepository;
        private readonly ICloudinaryService _cloudinaryService;

        public RoomImageService(IRoomImageRepository roomImageRepository, IPromptRepository promptRepository, IAIResultHistoryRepository aIResultHistoryRepository, ISavedDesignRepository savedDesignRepository, ICloudinaryService cloudinaryService)
        {
            _roomImageRepository = roomImageRepository;
            _promptRepository = promptRepository;
            _aiResultHistoryRepository = aIResultHistoryRepository;
            _savedDesignRepository = savedDesignRepository;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<string> SaveImageAsync(IFormFile image)
        {
            // Uploading to Cloudinary
            return await _cloudinaryService.UploadImageAsync(image);  // استدعاء خدمة رفع الصورة لـ Cloudinary
        }

        public async Task<RoomImage> GenerateRoomImageAsync(IFormFile imagePath, string descriptionText, RoomStyle roomStyle, RoomType roomType)
        {
            // رفع الصورة إلى Cloudinary بعد توليدها
            var imageUrl = await _cloudinaryService.UploadImageAsync(imagePath); // رفع الصورة إلى Cloudinary

            // حفظ الصورة في قاعدة البيانات
            var roomImage = new RoomImage
            {
                ImagePath = imageUrl,  // حفظ رابط الصورة من Cloudinary
                CreatedDate = DateTime.UtcNow
            };
            await _roomImageRepository.AddAsync(roomImage);

            // إنشاء الوصف وربطه بالصورة
            var description = new Prompt
            {
                DescriptionText = descriptionText,
                RoomStyle = roomStyle,
                RoomType = roomType,
                RoomImageId = roomImage.Id
            };
            await _promptRepository.AddAsync(description);

            return roomImage;
        }


        // Saving The AIResult History
        public async Task<AIResultHistory> SaveAIResultHistoryAsync(string imageUrl, string userId)
        {
            var aiResultHistory = new AIResultHistory
            {
                GeneratedImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow,
                ApplicationUserId = userId
            };

            await _aiResultHistoryRepository.AddAsync(aiResultHistory);
            return aiResultHistory;
        }

        //Saving The User SavedDesigns Generated From AI
        public async Task<SavedDesign> SaveUserDesignAsync(string imageUrl, string userId)
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
