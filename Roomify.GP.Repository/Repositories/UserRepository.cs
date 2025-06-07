using Roomify.GP.Core.Repositories.Contract;
using Microsoft.EntityFrameworkCore;
using Roomify.GP.Repository.Data.Contexts;
using Roomify.GP.Core.Entities.Identity;

namespace Roomify.GP.Repository.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApplicationUser> GetUserByIdAsync(Guid id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task AddUserAsync(ApplicationUser user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(ApplicationUser user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(Guid id)
        {
            var user = await GetUserByIdAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> UserExistsAsync(Guid? id)
        {
            return await _context.Users.AnyAsync(u => u.Id == id);

        }

        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task<List<ApplicationUser>> SearchUsersAsync(string query)
        {
            return await _context.Users
                .Where(u =>
                    u.UserName.Contains(query) ||
                    u.FullName.Contains(query))
                .ToListAsync();
        }


    }
}
