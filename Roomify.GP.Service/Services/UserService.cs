using Roomify.GP.Core.DTOs.ApplicationUser;
using Roomify.GP.Core.Entities.Identity;
using Roomify.GP.Core.Repositories.Contract;
using Roomify.GP.Core.Service.Contract;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Roomify.GP.Core.Services.Contract;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Roomify.GP.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;


        public UserService(IUserRepository userRepository, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<List<UserWithRolesDto>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var usersWithRoles = new List<UserWithRolesDto>();

            foreach (var user in users)
            {
                var userDto = _mapper.Map<UserWithRolesDto>(user);
                var roles = await _userManager.GetRolesAsync(user);
                userDto.Role = roles.FirstOrDefault() ?? "NormalUser";

                usersWithRoles.Add(userDto);
            }

            return usersWithRoles;
        }



        public async Task<UserWithRolesDto> GetUserByIdAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return null;

            // أول حاجة: نعمل المابينج
            var userDto = _mapper.Map<UserWithRolesDto>(user);

            // بعدين نجيب الرول ونحطها
            var roles = await _userManager.GetRolesAsync(user);
            userDto.Role = roles.FirstOrDefault() ?? "NormalUser";

            return userDto;
        }

        


        public async Task<ApplicationUser> UpdateUserAsync(Guid id, UserUpdateDto dto)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
                return null;

            _mapper.Map(dto, user);
            await _userRepository.UpdateUserAsync(user);
            return user;
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null) return false;
            await _userRepository.DeleteUserAsync(id);
            return true;
        }
    }
}
