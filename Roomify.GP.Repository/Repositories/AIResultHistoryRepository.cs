using Microsoft.EntityFrameworkCore;
using Roomify.GP.Core.Entities.AI;
using Roomify.GP.Core.Repositories.Contract;
using Roomify.GP.Repository.Data.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Repository.Repositories
{
    public class AIResultHistoryRepository : IAIResultHistoryRepository
    {
        private readonly AppDbContext _context;

        public AIResultHistoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AIResultHistory entity)
        {
            await _context.AIResultHistories.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<AIResultHistory>> GetByUserIdAsync(Guid userId)
        {
            return await _context.AIResultHistories
                .Where(h => h.ApplicationUserId == userId)
                .ToListAsync();
        }

        public async Task<List<AIResultHistory>> GetExpiredResultsAsync()
        {
            var expirationThreshold = DateTime.UtcNow.AddHours(-72);
            return await _context.AIResultHistories
                .Where(h => h.CreatedAt < expirationThreshold)
                .ToListAsync();
        }

        public async Task DeleteExpiredAsync()
        {
            var expirationThreshold = DateTime.UtcNow.AddDays(-5); // Adjust as needed

            // Get expired results with their related prompts
            var expiredResults = await _context.AIResultHistories
                .Include(a => a.Prompt) // Include related prompts
                .Where(a => a.CreatedAt < expirationThreshold)
                .ToListAsync();

            if (expiredResults.Any())
            {
                // First, delete all related prompts
                var promptsToDelete = expiredResults.Select(a => a.Prompt).ToList();
                if (promptsToDelete.Any())
                {
                    _context.Prompts.RemoveRange(promptsToDelete);
                }

                // Then delete the AIResultHistories
                _context.AIResultHistories.RemoveRange(expiredResults);

                await _context.SaveChangesAsync();
            }
        }
    }
}
