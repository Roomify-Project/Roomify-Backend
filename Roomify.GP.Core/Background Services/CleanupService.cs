using Microsoft.Extensions.DependencyInjection;
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
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(24); // يعمل كل 24 ساعة

        public CleanupService(IServiceScopeFactory serviceScopeFactory, ICloudinaryService cloudinaryService)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _cloudinaryService = cloudinaryService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (IServiceScope scope = _serviceScopeFactory.CreateScope())
                {
                    var aiResultHistoryRepository = scope.ServiceProvider.GetRequiredService<IAIResultHistoryRepository>();
                    var cloudinaryService = scope.ServiceProvider.GetRequiredService<ICloudinaryService>();

                    // Use the scoped repository and service
                    await CleanupExpiredAIResultsAsync(aiResultHistoryRepository, cloudinaryService);
                }
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
        }

        private async Task CleanupExpiredAIResultsAsync(IAIResultHistoryRepository aiResultHistoryRepository, ICloudinaryService cloudinaryService)
        {
            var expiredResults = await aiResultHistoryRepository.GetExpiredResultsAsync();

            foreach (var result in expiredResults)
            {
                if (!string.IsNullOrEmpty(result.generatedImageUrl))
                {
                    await cloudinaryService.DeleteImageAsync(result.generatedImageUrl);
                }
            }

            await aiResultHistoryRepository.DeleteExpiredAsync();
        }
    }
}
