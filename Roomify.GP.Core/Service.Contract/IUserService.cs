using Roomify.GP.Core.DTOs.ApplicationUser;
using Roomify.GP.Core.DTOs.User;
using Roomify.GP.Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Services.Contract
{
    public interface IUserService
    {
        Task<List<UserWithRolesDto>> GetAllUsersAsync();

        Task<UserWithRolesDto> GetUserByIdAsync(Guid id);

        Task<string?> UpdateUserAsync(Guid id, UserUpdateDto dto);


        Task<bool> DeleteUserAsync(Guid id);

    }
}
