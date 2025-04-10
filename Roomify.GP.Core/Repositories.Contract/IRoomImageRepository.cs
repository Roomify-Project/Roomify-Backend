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
        Task AddAsync(RoomImage entity);  // لإضافة صورة جديدة
        Task<RoomImage> GetByIdAsync(Guid id);  // للحصول على صورة بناءً على الـ ID
        Task<List<RoomImage>> GetAllAsync();  // للحصول على جميع الصور
        Task DeleteAsync(Guid id);  // لحذف صورة
    }
}
