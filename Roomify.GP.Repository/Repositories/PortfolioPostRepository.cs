using Microsoft.EntityFrameworkCore;
using Roomify.GP.Core.Entities;
using Roomify.GP.Core.Repositories.Contract;
using Roomify.GP.Repository.Data.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Repository.Repositories
{
    public class PortfolioPostRepository : IPortfolioPostRepository
    {
        private readonly AppDbContext _context;
        public PortfolioPostRepository(AppDbContext context)
        {
            _context = context;
        }


        public async Task<IEnumerable<PortfolioPost>> GetAllAsync()
        {
            return await _context.PortfolioPosts.Include( P => P.User).ToListAsync();
        }

        public async Task<IEnumerable<PortfolioPost>> GetByUserIdAsync(Guid userId)
        {
            return await _context.PortfolioPosts
     .Where(p => p.UserId == userId.ToString())
     .ToListAsync();

        }

        public async Task<PortfolioPost> GetByIdAsync(Guid id)
        {
            return await _context.PortfolioPosts.Include(P => P.User).FirstOrDefaultAsync(P => P.Id == id);
        }

        public async Task AddAsync(PortfolioPost post)
        {
            await _context.PortfolioPosts.AddAsync(post);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PortfolioPost post)
        {
            _context.PortfolioPosts.Update(post);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var post = await _context.PortfolioPosts.FindAsync(id);
            if (post != null)
            {
                _context.PortfolioPosts.Remove(post);
                await _context.SaveChangesAsync();
            }
        }
    }
}
