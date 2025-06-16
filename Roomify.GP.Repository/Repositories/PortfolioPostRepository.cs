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
            return await _context.PortfolioPosts
            .Include(p => p.ApplicationUser)
            .Include(p => p.Comments)
            .Include(p => p.Likes)
            .ToListAsync();
        }

        public async Task<PortfolioPost> GetByIdAsync(Guid id)
        {
            return await _context.PortfolioPosts
                .Include(p => p.ApplicationUser)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.ApplicationUser) // ✅ هنا الإضافة المهمة
                .Include(p => p.Likes)
                .FirstOrDefaultAsync(p => p.Id == id);
        }


        public async Task<IEnumerable<PortfolioPost>> GetByUserIdAsync(Guid userId)
        {
            return await _context.PortfolioPosts
            .Where(p => p.ApplicationUserId == userId)
            .Include(p => p.Comments)
            .Include(p => p.Likes)
            .ToListAsync();

        }

        public async Task AddAsync(Guid userId, PortfolioPost post)
        {
            await _context.PortfolioPosts.AddAsync(post);
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

        
        public async Task<IEnumerable<Comment>> GetAllCommentsAsync(Guid postId) => await GetAllByPostIdAsync(postId);
        private async Task<IEnumerable<Comment>> GetAllByPostIdAsync(Guid postId)
        {
            return await _context.Comments
                .Where(c => c.PortfolioPostId == postId && !c.IsDeleted)
                .Include(c => c.ApplicationUser)
                .ToListAsync();
        }


        public async Task<bool> LikeExistsAsync(Guid postId, Guid userId)
        {
            return await _context.Likes.AnyAsync(l => l.PortfolioPostId == postId && l.ApplicationUserId == userId);
        }

        public async Task AddLikeAsync(Like like)
        {
            await _context.Likes.AddAsync(like);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveLikeAsync(Guid postId, Guid userId)
        {
            var like = await _context.Likes.FirstOrDefaultAsync(l => l.PortfolioPostId == postId && l.ApplicationUserId == userId);
            if (like != null) { _context.Likes.Remove(like); await _context.SaveChangesAsync(); }
        }

        public async Task<int> GetLikesCountAsync(Guid postId) => await _context.Likes.CountAsync(l => l.PortfolioPostId == postId);
    }
}

