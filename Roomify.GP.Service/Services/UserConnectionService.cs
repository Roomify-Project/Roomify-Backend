// Roomify.GP.Service/Services/UserConnectionService.cs
using Roomify.GP.Core.Entities.Identity;
using Roomify.GP.Core.Repositories.Contract;
using Roomify.GP.Core.Services.Contract;

namespace Roomify.GP.Service.Services
{
    public class UserConnectionService : IUserConnectionService
    {
        private readonly IUserConnectionRepository _repository;

        public UserConnectionService(IUserConnectionRepository repository)
        {
            _repository = repository;
        }

        public Task AddConnectionAsync(UserConnection connection)
        {
            return _repository.AddAsync(connection);
        }

        public Task RemoveConnectionAsync(string connectionId)
        {
            return _repository.RemoveAsync(connectionId);
        }
        public async Task<bool> IsUserOnlineAsync(Guid userId)
        {
            var connections = await _repository.GetByUserIdAsync(userId);
            return connections.Any();
        }

    }
}
