using Microsoft.EntityFrameworkCore;
using Roomify.GP.Core.Entities.AI.RoomImage;
using Roomify.GP.Core.Repositories.Contract;
using Roomify.GP.Repository.Data.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Repository.Repositories
{
    public class PromptRepository : IPromptRepository
    {
        private readonly AppDbContext _context;

        public PromptRepository(AppDbContext context)
        {
            _context = context;
        }

        // Basic CRUD operations
        public async Task<Prompt> GetByIdAsync(Guid id)
        {
            return await _context.Prompts.FindAsync(id);
        }

        public async Task<List<Prompt>> GetAllAsync()
        {
            return await _context.Prompts.ToListAsync();
        }

        public async Task<Prompt> AddAsync(Prompt entity)
        {
            await _context.Prompts.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(Prompt entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.Prompts.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        // Additional prompt-specific methods
        public async Task<List<Prompt>> GetByRoomImageIdAsync(Guid roomImageId)
        {
            return await _context.Prompts
                .Where(p => p.RoomImageId == roomImageId)
                .ToListAsync();
        }

        public async Task<List<Prompt>> GetByAIResultHistoryIdAsync(Guid aiResultHistoryId)
        {
            return await _context.Prompts
                .Where(p => p.AIResultHistoryId == aiResultHistoryId)
                .ToListAsync();
        }

        public async Task<List<Prompt>> GetByUserIdAsync(Guid userId)
        {
            // This assumes you can get to the user via navigation properties
            // Adjust the query based on your actual data model
            return await _context.Prompts
                .Where(p =>
                    (p.RoomImage != null && p.RoomImage.ApplicationUserId == userId) ||
                    (p.AIResultHistory != null && p.AIResultHistory.ApplicationUserId == userId))
                .ToListAsync();
        }
    }
}