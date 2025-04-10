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

        public async Task<List<AIResultHistory>> GetByUserIdAsync(string userId)
        {
            return await _context.AIResultHistories
                .Where(h => h.ApplicationUserId == userId)
                .ToListAsync();
        }

        public async Task DeleteExpiredAsync()
        {
            var expiredResults = await _context.AIResultHistories
                .Where(h => h.IsExpired)
                .ToListAsync();

            _context.AIResultHistories.RemoveRange(expiredResults);
            await _context.SaveChangesAsync();
        }
    }
}
