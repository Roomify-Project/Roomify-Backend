using Microsoft.EntityFrameworkCore;
using Roomify.GP.Core.Entities.Identity;
using Roomify.GP.Core.Repositories.Contract;
using Roomify.GP.Repository.Data.Contexts;
using System;
using System.Threading.Tasks;

namespace Roomify.GP.Repository.Repositories
{
    public class PendingRegistrationRepository : IPendingRegistrationRepository
    {
        private readonly AppDbContext _context;

        public PendingRegistrationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(PendingRegistration registration)
        {
            await _context.PendingRegistrations.AddAsync(registration);
        }

        public async Task<PendingRegistration> GetByIdAsync(Guid id)
        {
            return await _context.PendingRegistrations.FindAsync(id);
        }

        public async Task<PendingRegistration> GetByEmailAsync(string email)
        {
            return await _context.PendingRegistrations
                .FirstOrDefaultAsync(r => r.Email.ToLower() == email.ToLower());
        }

        public async Task UpdateAsync(PendingRegistration registration)
        {
            _context.PendingRegistrations.Update(registration);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(PendingRegistration registration)
        {
            _context.PendingRegistrations.Remove(registration);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
