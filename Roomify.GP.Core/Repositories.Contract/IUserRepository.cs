using Roomify.GP.Core.Entities;
using Roomify.GP.Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Repositories.Contract
{
    public interface IUserRepository
    {
        Task<ApplicationUser> GetUserByIdAsync(Guid id);
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
        Task AddUserAsync(ApplicationUser user);
        Task UpdateUserAsync(ApplicationUser user);
        Task DeleteUserAsync(Guid id);
        Task<bool> UserExistsAsync(Guid? id);
        Task<ApplicationUser> GetUserByEmailAsync(string email);
        Task<List<ApplicationUser>> SearchUsersAsync(string query);


    }
}
