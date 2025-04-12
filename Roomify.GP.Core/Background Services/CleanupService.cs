using Microsoft.Extensions.Hosting;
using Roomify.GP.Core.Repositories.Contract;
using Roomify.GP.Core.Service.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Background_Services
{
    public class CleanupService : BackgroundService
    {
        private readonly IAIResultHistoryRepository _aiResultHistoryRepository;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(24); // يعمل كل 24 ساعة

        public CleanupService(IAIResultHistoryRepository aiResultHistoryRepository, ICloudinaryService cloudinaryService)
        {
            _aiResultHistoryRepository = aiResultHistoryRepository;
            _cloudinaryService = cloudinaryService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // تنظيف العناصر القديمة من AIResultHistory
                await CleanupExpiredAIResultsAsync();

                // الانتظار حتى الدورة التالية
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
        }

        private async Task CleanupExpiredAIResultsAsync()
        {
            // الحصول على جميع AIResultHistory القديمة التي مر عليها أكثر من 72 ساعة
            var expiredResults = await _aiResultHistoryRepository.GetByUserIdAsync(""); // يمكن تعديلها للحصول على بيانات مستخدم محدد أو حسب الحاجة

            foreach (var result in expiredResults)
            {
                // حذف الصورة من Cloudinary باستخدام public ID
                await _cloudinaryService.DeleteImageAsync(result.GeneratedImageUrl);
            }

            // يمكن أيضًا حذف السجلات من قاعدة البيانات بعد الحذف
            await _aiResultHistoryRepository.DeleteExpiredAsync();
        }
    }
}
