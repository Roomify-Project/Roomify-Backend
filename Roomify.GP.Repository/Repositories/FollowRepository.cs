using Roomify.GP.Core.Entities.Identity;
using Roomify.GP.Core.Repositories.Contract;
using Roomify.GP.Repository.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Roomify.GP.Core.Entities;

namespace Roomify.GP.Repository.Repositories
{
    public class FollowRepository : IFollowRepository
    {
        private readonly AppDbContext _context;

        public FollowRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddFollowAsync(UserFollow follow)
        {
            _context.UserFollows.Add(follow);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveFollowAsync(UserFollow follow)
        {
            _context.UserFollows.Remove(follow);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsFollowingAsync(Guid followerId, Guid followingId)
        {
            return await _context.UserFollows
                .AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
        }

        public async Task<int> GetFollowersCountAsync(Guid userId)
        {
            return await _context.UserFollows
                .CountAsync(f => f.FollowingId == userId);
        }

        public async Task<int> GetFollowingCountAsync(Guid userId)
        {
            return await _context.UserFollows
                .CountAsync(f => f.FollowerId == userId);
        }

        public async Task<List<ApplicationUser>> GetFollowersAsync(Guid userId)
        {
            return await _context.UserFollows
                .Where(f => f.FollowingId == userId)
                .Select(f => f.Follower)
                .ToListAsync();
        }

        public async Task<List<ApplicationUser>> GetFollowingAsync(Guid userId)
        {
            return await _context.UserFollows
                .Where(f => f.FollowerId == userId)
                .Select(f => f.Following)
                .ToListAsync();
        }
    }
}
