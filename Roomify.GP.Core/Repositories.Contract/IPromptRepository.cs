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
        Task AddAsync(Prompt entity);  // لإضافة وصف جديد
        Task<Prompt> GetByIdAsync(Guid id);  // للحصول على وصف بناءً على الـ ID
        Task<List<Prompt>> GetAllAsync();  // للحصول على جميع الأوصاف
        Task DeleteAsync(Guid id);  // لحذف وصف
    }
}
