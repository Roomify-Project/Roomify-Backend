using Roomify.GP.Core.DTOs.ApplicationUser;
using Roomify.GP.Core.Entities.Identity;
using Roomify.GP.Core.Repositories.Contract;
using Roomify.GP.Core.Service.Contract;
using Roomify.GP.Core.Services.Contract;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Roomify.GP.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICloudinaryService _cloudinaryService;

        public UserService(
            IUserRepository userRepository,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            ICloudinaryService cloudinaryService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _userManager = userManager;
            _cloudinaryService = cloudinaryService;
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

            var userDto = _mapper.Map<UserWithRolesDto>(user);
            var roles = await _userManager.GetRolesAsync(user);
            userDto.Role = roles.FirstOrDefault() ?? "NormalUser";

            return userDto;
        }

        public async Task<string?> UpdateUserAsync(Guid id, UserUpdateDto dto)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
                return null;

            user.FullName = dto.FullName ?? user.FullName;
            user.Bio = dto.Bio ?? user.Bio;
            user.Email = dto.Email ?? user.Email;
            user.UserName = dto.UserName ?? user.UserName;

            if (dto.ProfileImage != null && dto.ProfileImage.Length > 0)
            {
                var imageUrl = await _cloudinaryService.UploadImageAsync(dto.ProfileImage);
                user.ProfilePicture = imageUrl;
            }

            await _userRepository.UpdateUserAsync(user);
            return user.ProfilePicture;
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
