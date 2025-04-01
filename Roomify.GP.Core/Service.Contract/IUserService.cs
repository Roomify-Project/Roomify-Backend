﻿using Roomify.GP.Core.DTOs.ApplicationUser;
using Roomify.GP.Core.DTOs.User;
using Roomify.GP.Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Services.Contract
{
    public interface IUserService
    {
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
        Task<ApplicationUser> GetUserByIdAsync(Guid id);
        Task<ApplicationUser> UpdateUserAsync(Guid id, UserUpdateDto dto);
        Task<bool> DeleteUserAsync(Guid id);

    }
}
