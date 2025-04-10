using Roomify.GP.Core.Entities.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Repositories.Contract
{
    public interface IAIResultHistoryRepository
    {
        Task AddAsync(AIResultHistory entity);  
        Task<List<AIResultHistory>> GetByUserIdAsync(string userId); 
        Task DeleteExpiredAsync(); 
    }
}
