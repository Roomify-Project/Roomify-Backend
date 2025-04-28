using Roomify.GP.Core.Entities;
using Roomify.GP.Core.Entities.Identity;
using Roomify.GP.Core.Repositories.Contract;
using Roomify.GP.Core.Service.Contract;

namespace Roomify.GP.Service.Services
{
    public class FollowService : IFollowService
    {
        private readonly IFollowRepository _followRepository;

        public FollowService(IFollowRepository followRepository)
        {
            _followRepository = followRepository;
        }

        public async Task<bool> FollowAsync(Guid followerId, Guid followingId)
        {
            if (followerId == followingId)
                throw new InvalidOperationException("You cannot follow yourself.");

            var isAlreadyFollowing = await _followRepository.IsFollowingAsync(followerId, followingId);
            if (isAlreadyFollowing)
                return false;

            var follow = new UserFollow
            {
                FollowerId = followerId,
                FollowingId = followingId,
                FollowedAt = DateTime.UtcNow
            };

            await _followRepository.AddFollowAsync(follow);
            return true;
        }

        public async Task<bool> UnfollowAsync(Guid followerId, Guid followingId)
        {
            var isFollowing = await _followRepository.IsFollowingAsync(followerId, followingId);
            if (!isFollowing)
                return false;

            var follow = new UserFollow
            {
                FollowerId = followerId,
                FollowingId = followingId
            };

            await _followRepository.RemoveFollowAsync(follow);
            return true;
        }

        public async Task<int> GetFollowersCountAsync(Guid userId)
        {
            return await _followRepository.GetFollowersCountAsync(userId);
        }

        public async Task<int> GetFollowingCountAsync(Guid userId)
        {
            return await _followRepository.GetFollowingCountAsync(userId);
        }

        public async Task<bool> IsFollowingAsync(Guid followerId, Guid followingId)
        {
            return await _followRepository.IsFollowingAsync(followerId, followingId);
        }
        public async Task<List<ApplicationUser>> GetFollowersAsync(Guid userId)
        {
            return await _followRepository.GetFollowersAsync(userId);
        }

        public async Task<List<ApplicationUser>> GetFollowingAsync(Guid userId)
        {
            return await _followRepository.GetFollowingAsync(userId);
        }
    }
}
