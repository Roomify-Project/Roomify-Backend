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
    public class SavedDesignRepository : ISavedDesignRepository
    {
        private readonly AppDbContext _context;

        public SavedDesignRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(SavedDesign entity)
        {
            await _context.SavedDesigns.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<SavedDesign>> GetByUserIdAsync(Guid userId)
        {
            return await _context.SavedDesigns
                .Where(s => s.ApplicationUserId == userId).Include(s => s.ApplicationUser.FullName).Include(s => s.ApplicationUser.ProfilePicture)
                .ToListAsync();
        }

        public async Task<List<SavedDesign>> GetByUserIdWithUserInfoAsync(Guid userId)
        {
            return await _context.SavedDesigns
                .Include(s => s.ApplicationUser)
                .Where(s => s.ApplicationUserId == userId)
                .ToListAsync();
        }

        public async Task<List<SavedDesign>> GetAllWithUserInfoAsync()
        {
            return await _context.SavedDesigns
                .Include(s => s.ApplicationUser)
                .ToListAsync();
        }

        public async Task<SavedDesign> GetByIdWithUserInfoAsync(Guid id)
        {
            return await _context.SavedDesigns
                .Include(s => s.ApplicationUser)
                .FirstOrDefaultAsync(s => s.Id == id);
        }
    }
}