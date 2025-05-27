using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Roomify.GP.Core.Repositories.Contract;
using Roomify.GP.Core.Service.Contract;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Background_Services
{
    public class CleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<CleanupService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(24); // Run once a day

        public CleanupService(IServiceScopeFactory serviceScopeFactory, ILogger<CleanupService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AI Results Cleanup Service running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredResultsAsync();
                    _logger.LogInformation("Completed AI results cleanup.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during AI results cleanup.");
                }

                // Wait for the next cleanup interval
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
        }

        private async Task CleanupExpiredResultsAsync()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var aiResultHistoryRepository = scope.ServiceProvider.GetRequiredService<IAIResultHistoryRepository>();
                var cloudinaryService = scope.ServiceProvider.GetRequiredService<ICloudinaryService>();

                // Get expired results
                var expiredResults = await aiResultHistoryRepository.GetExpiredResultsAsync();
                _logger.LogInformation($"Found {expiredResults.Count} expired AI results to clean up.");

                // Delete images from Cloudinary
                foreach (var result in expiredResults)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(result.GeneratedImageUrl))
                        {
                            await cloudinaryService.DeleteImageAsync(result.GeneratedImageUrl);
                            _logger.LogInformation($"Deleted image from Cloudinary: {result.GeneratedImageUrl}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error deleting image from Cloudinary: {result.GeneratedImageUrl}");
                        // Continue with other images even if one fails
                    }
                }

                // Delete expired results from database
                await aiResultHistoryRepository.DeleteExpiredAsync();
            }
        }
    }
}