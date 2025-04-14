using Roomify.GP.Core.Entities;
using Roomify.GP.Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Repositories.Contract
{
    public interface IFollowRepository
    {
        Task AddFollowAsync(UserFollow follow);
        Task RemoveFollowAsync(UserFollow follow);

        Task<bool> IsFollowingAsync(Guid followerId, Guid followingId);

        Task<int> GetFollowersCountAsync(Guid userId);
        Task<int> GetFollowingCountAsync(Guid userId);

        Task<List<ApplicationUser>> GetFollowersAsync(Guid userId);
        Task<List<ApplicationUser>> GetFollowingAsync(Guid userId);
    }
}
    