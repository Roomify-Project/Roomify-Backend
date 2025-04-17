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

        public async Task AddAsync(Prompt entity)
        {
            await _context.Prompts.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<Prompt> GetByIdAsync(Guid id)
        {
            return await _context.Prompts
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<List<Prompt>> GetAllAsync()
        {
            return await _context.Prompts.ToListAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var description = await _context.Prompts.FindAsync(id);
            if (description != null)
            {
                _context.Prompts.Remove(description);
                await _context.SaveChangesAsync();
            }
        }
    }
}
