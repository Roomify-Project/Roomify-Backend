using Roomify.GP.Core.Entities.AI.RoomImage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Repositories.Contract
{
    public interface IPromptRepository
    {
        Task AddAsync(Prompt entity);  
        Task<Prompt> GetByIdAsync(Guid id); 
        Task<List<Prompt>> GetAllAsync(); 
        Task DeleteAsync(Guid id); 
    }
}
