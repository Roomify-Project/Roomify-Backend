using Roomify.GP.Core.Entities.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Repositories.Contract
{
    public interface ISavedDesignRepository
    {
        Task AddAsync(SavedDesign entity);
        Task<List<SavedDesign>> GetByUserIdAsync(Guid userId);
    }
}
