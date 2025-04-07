using Roomify.GP.Core.Entities;
using Roomify.GP.Core.Repositories.Contract;
using Roomify.GP.Core.Service.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Service.Services
{
    public class PortfolioPostService : IPortfolioPostService
    {
        private readonly IPortfolioPostRepository _repo;
        public PortfolioPostService(IPortfolioPostRepository repo)
        {
            _repo = repo;
        }


        public async Task<IEnumerable<PortfolioPost>> GetAllAsync() => await _repo.GetAllAsync();
        public async Task<IEnumerable<PortfolioPost>> GetByUserIdAsync(Guid userId) => await _repo.GetByUserIdAsync(userId);
        public async Task<PortfolioPost> GetByIdAsync(Guid id) => await _repo.GetByIdAsync(id);
        public async Task AddAsync(Guid userId, PortfolioPost post) => await _repo.AddAsync(userId, post);
        public async Task DeleteAsync(Guid id) => await _repo.DeleteAsync(id);
    }
}
