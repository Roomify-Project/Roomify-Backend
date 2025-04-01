using Roomify.GP.Core.DTOs.ApplicationUser;
using Roomify.GP.Core.Entities.Identity;
using Roomify.GP.Core.Repositories.Contract;
using Roomify.GP.Core.Service.Contract;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Roomify.GP.Core.Services.Contract;

namespace Roomify.GP.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }

        public async Task<ApplicationUser> GetUserByIdAsync(Guid id)
        {
            return await _userRepository.GetUserByIdAsync(id);
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
