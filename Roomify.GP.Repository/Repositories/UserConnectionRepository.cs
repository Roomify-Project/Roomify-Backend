// Roomify.GP.Repository/Repositories/UserConnectionRepository.cs
using Microsoft.EntityFrameworkCore;
using Roomify.GP.Core.Entities.Identity;
using Roomify.GP.Core.Repositories.Contract;
using Roomify.GP.Repository.Data.Contexts;

namespace Roomify.GP.Repository.Repositories
{
    public class UserConnectionRepository : IUserConnectionRepository
    {
        private readonly AppDbContext _context;

        public UserConnectionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(UserConnection connection)
        {
            _context.UserConnections.Add(connection);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(string connectionId)
        {
            var conn = _context.UserConnections.FirstOrDefault(x => x.ConnectionId == connectionId);
            if (conn != null)
            {
                _context.UserConnections.Remove(conn);
                await _context.SaveChangesAsync();
            }

        }

        public async Task<List<UserConnection>> GetByUserIdAsync(Guid userId)
        {
            return await _context.UserConnections
                                 .Where(c => c.UserId == userId)
                                 .ToListAsync();
        }

    }
}
