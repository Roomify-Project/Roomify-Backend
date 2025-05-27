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
                .Where(s => s.ApplicationUserId == userId)
                .ToListAsync();
        }
    }
}
