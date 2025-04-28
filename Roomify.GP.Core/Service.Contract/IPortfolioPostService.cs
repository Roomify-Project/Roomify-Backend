using Roomify.GP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Service.Contract
{
    public interface IPortfolioPostService
    {
        Task<IEnumerable<PortfolioPost>> GetAllAsync();
        Task<IEnumerable<PortfolioPost>> GetByUserIdAsync(Guid userId);
        Task<PortfolioPost> GetByIdAsync(Guid id);
        Task AddAsync(Guid userId, PortfolioPost post);
        Task DeleteAsync(Guid id);
    }
}
