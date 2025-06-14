﻿using Microsoft.EntityFrameworkCore;
using Roomify.GP.Core.Entities;
using Roomify.GP.Core.Repositories.Contract;
using Roomify.GP.Repository.Data.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Roomify.GP.Repository.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly AppDbContext _context;

        public CommentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Comment>> GetAllByPostIdAsync(Guid postId)
        {
            return await _context.Comments
                .Where(c => c.PortfolioPostId == postId && !c.IsDeleted)
                .Include(c => c.ApplicationUser)
                .Include(c => c.PortfolioPost)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetAllBySavedDesignIdAsync(Guid designId)
        {
             return await _context.Comments
                .Where(c => c.SavedDesignId == designId && !c.IsDeleted)
                .Include(c => c.ApplicationUser)
                .Include(c => c.SavedDesign)
                .ToListAsync();
        }

        public async Task<Comment> GetByIdAsync(Guid id)
        {
            return await _context.Comments
                .Include(c => c.ApplicationUser)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task AddAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Comment comment)
        {
            comment.UpdatedAt = DateTime.UtcNow;
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.Comments.FindAsync(id);
            if (entity != null) 
            { 
                _context.Comments.Remove(entity); 
                await _context.SaveChangesAsync(); 
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Comments.AnyAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<bool> IsAuthorizedUserAsync(Guid commentId, Guid userId)
        {
            return await _context.Comments
                .AnyAsync(c => c.Id == commentId && c.ApplicationUserId == userId && !c.IsDeleted);
        } 
    }
}  