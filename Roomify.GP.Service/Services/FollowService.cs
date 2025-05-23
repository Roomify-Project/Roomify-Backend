using Roomify.GP.Core.Entities;
using Roomify.GP.Core.Entities.Identity;
using Roomify.GP.Core.Repositories.Contract;
using Roomify.GP.Core.Service.Contract;
using Roomify.GP.Core.Services.Contract;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Roomify.GP.Service.Services
{
    public class FollowService : IFollowService
    {
        private readonly IFollowRepository _followRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;

        public FollowService(
            IFollowRepository followRepository,
            IUserRepository userRepository,
            INotificationService notificationService)
        {
            _followRepository = followRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
        }

        public async Task<bool> FollowAsync(Guid followerId, Guid followingId)
        {
            // التحقق من عدم متابعة الشخص لنفسه
            if (followerId == followingId)
                throw new InvalidOperationException("You cannot follow yourself.");

            // التحقق إذا كان المستخدم يتابع بالفعل
            var isAlreadyFollowing = await _followRepository.IsFollowingAsync(followerId, followingId);
            if (isAlreadyFollowing)
                return false;

            // إنشاء علاقة متابعة جديدة
            var follow = new UserFollow
            {
                FollowerId = followerId,
                FollowingId = followingId,
                FollowedAt = DateTime.UtcNow
            };

            // حفظ المتابعة في قاعدة البيانات
            await _followRepository.AddFollowAsync(follow);

            // بعد نجاح المتابعة، إرسال إشعار للشخص الذي تمت متابعته
            var follower = await _userRepository.GetUserByIdAsync(followerId);
            await _notificationService.CreateFollowNotificationAsync(
                followingId,
                followerId,
                follower.UserName);

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