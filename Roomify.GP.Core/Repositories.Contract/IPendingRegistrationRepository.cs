using Roomify.GP.Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Repositories.Contract
{
    public interface IPendingRegistrationRepository
    {
        Task AddAsync(PendingRegistration registration);
        Task<PendingRegistration> GetByIdAsync(Guid id);
        Task<PendingRegistration> GetByEmailAsync(string email);
        Task UpdateAsync(PendingRegistration registration);
        Task DeleteAsync(PendingRegistration registration);
        Task SaveChangesAsync();
    }
}
