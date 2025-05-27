using Microsoft.EntityFrameworkCore;
using Roomify.GP.Core.Entities.AI.RoomImage;
using Roomify.GP.Core.Repositories.Contract;
using Roomify.GP.Repository.Data.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roomify.GP.Repository.Repositories
{
    public class RoomImageRepository : IRoomImageRepository
    {
        private readonly AppDbContext _context;

        public RoomImageRepository(AppDbContext context)
        {
            _context = context;
        }


        public async Task AddAsync(RoomImage entity)
        {
            await _context.RoomImages.AddAsync(entity);
            await _context.SaveChangesAsync();
        }
        public async Task<RoomImage> GetByIdAsync(Guid id)
        {
            return await _context.RoomImages
            .Include(R => R.Prompt) 
            .FirstOrDefaultAsync(R => R.Id == id);
        }
        public async Task<List<RoomImage>> GetAllAsync()
        {
            return await _context.RoomImages.ToListAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var roomImage = await _context.RoomImages.FindAsync(id);
            if (roomImage != null)
            {
                _context.RoomImages.Remove(roomImage);
                await _context.SaveChangesAsync();
            }
        }


    }
}
