using Roomify.GP.Core.Entities.Identity;
using System;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Service.Contract
{

    public interface IFollowService
    {
        Task<bool> FollowAsync(Guid followerId, Guid followingId);
        Task<bool> UnfollowAsync(Guid followerId, Guid followingId);
        Task<int> GetFollowersCountAsync(Guid userId);
        Task<int> GetFollowingCountAsync(Guid userId);
        Task<bool> IsFollowingAsync(Guid followerId, Guid followingId);
        Task<List<ApplicationUser>> GetFollowersAsync(Guid userId);
        Task<List<ApplicationUser>> GetFollowingAsync(Guid userId);
    }
}
