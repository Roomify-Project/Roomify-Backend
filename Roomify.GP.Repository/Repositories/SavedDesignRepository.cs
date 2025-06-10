using Microsoft.EntityFrameworkCore;
using Roomify.GP.Core.Entities;
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
    public class SavedDesignRepository : ISavedDesignRepository
    {
        private readonly AppDbContext _context;

        public SavedDesignRepository(AppDbContext context)
        {
            _context = context;
        }
         

        public async Task<List<SavedDesign>> GetAllWithUserInfoAsync()
        {
            return await _context.SavedDesigns
                .Include(s => s.ApplicationUser)
                .Include(d => d.Comments)
                .Include(d => d.Likes)
                .ToListAsync();
        }

        public async Task<List<SavedDesign>> GetByUserIdWithUserInfoAsync(Guid userId)
        {
            return await _context.SavedDesigns
                .Where(s => s.ApplicationUserId == userId)
                .Include(s => s.ApplicationUser)
                .Include(d => d.Comments)
                .Include(d => d.Likes)
                .ToListAsync();
        }

        public async Task<SavedDesign> GetByIdWithUserInfoAsync(Guid id)
        {
            return await _context.SavedDesigns
                .Include(s => s.ApplicationUser)
                .Include(d => d.Comments)
                .Include(d => d.Likes)
                .FirstOrDefaultAsync(s => s.Id == id);
        }


        public async Task AddAsync(SavedDesign entity)
        {
            await _context.SavedDesigns.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.SavedDesigns.FindAsync(id);
            if (entity != null)
            { 
                _context.SavedDesigns.Remove(entity); 
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Comment>> GetAllCommentsAsync(Guid designId)
        {
            return await _context.Comments
                .Where(c => c.SavedDesignId == designId && !c.IsDeleted)
                .Include(c => c.ApplicationUser)
                .ToListAsync();
        }

        public async Task<bool> LikeExistsAsync(Guid designId, Guid userId)
        {
            return await _context.Likes.AnyAsync(l => l.SavedDesignId == designId && l.ApplicationUserId == userId);
        }

        public async Task AddLikeAsync(Like like)
        {
            await _context.Likes.AddAsync(like);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveLikeAsync(Guid designId, Guid userId)
        {
            var like = await _context.Likes.FirstOrDefaultAsync(l => l.SavedDesignId == designId && l.ApplicationUserId == userId);
            if (like != null) { _context.Likes.Remove(like); await _context.SaveChangesAsync(); }
        }

        public async Task<int> GetLikesCountAsync(Guid designId) => await _context.Likes.CountAsync(l => l.SavedDesignId == designId);
    }
}