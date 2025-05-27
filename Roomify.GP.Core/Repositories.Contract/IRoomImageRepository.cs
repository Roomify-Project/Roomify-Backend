using Roomify.GP.Core.Entities.AI.RoomImage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Core.Repositories.Contract
{
    public interface IRoomImageRepository
    {
        Task AddAsync(RoomImage entity);
        Task<RoomImage> GetByIdAsync(Guid id);
        Task<List<RoomImage>> GetAllAsync();
        Task DeleteAsync(Guid id);
    }
}
