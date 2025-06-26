// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.Base.MessageService.Interfaces;
using msih.p4g.Server.Features.Base.MessageService.Models;

namespace msih.p4g.Server.Features.Base.MessageService.Repositories
{
    /// <summary>
    /// Repository implementation for Message entities
    /// </summary>
    public class MessageRepository : GenericRepository<Message, ApplicationDbContext>, IMessageRepository
    {
        public MessageRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Message>> GetPendingMessagesAsync(int limit = 50, int maxRetries = 3)
        {
            return await _dbSet
                .Where(m => !m.IsSent && m.IsActive &&
                          m.RetryCount < maxRetries &&
                          (m.ScheduledFor == null || m.ScheduledFor <= DateTime.UtcNow))
                .OrderBy(m => m.CreatedOn)
                .Take(limit)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Message>> GetScheduledMessagesAsync(DateTime before, int limit = 50, int maxRetries = 3)
        {
            return await _dbSet
                .Where(m => !m.IsSent && m.IsActive &&
                          m.ScheduledFor != null && m.ScheduledFor <= before &&
                          m.RetryCount < maxRetries)
                .OrderBy(m => m.ScheduledFor)
                .Take(limit)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Message>> GetFailedMessagesAsync(int limit = 50, int maxRetries = 3)
        {
            return await _dbSet
                .Where(m => !m.IsSent && m.IsActive &&
                          m.RetryCount > 0 && m.RetryCount < maxRetries &&
                          !string.IsNullOrEmpty(m.ErrorMessage))
                .OrderBy(m => m.ModifiedOn ?? m.CreatedOn)
                .Take(limit)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Message>> GetMessagesByTypeAsync(string messageType, int limit = 50)
        {
            return await _dbSet
                .Where(m => m.MessageType == messageType && m.IsActive)
                .OrderByDescending(m => m.CreatedOn)
                .Take(limit)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Message>> GetMessagesByRecipientAsync(string recipient, int limit = 50)
        {
            return await _dbSet
                .Where(m => m.To == recipient && m.IsActive)
                .OrderByDescending(m => m.CreatedOn)
                .Take(limit)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<Message> UpdateMessageStatusAsync(int id, bool isSuccess, string errorMessage = "")
        {
            var message = await _dbSet.FindAsync(id);
            if (message == null)
            {
                throw new ArgumentException($"Message with ID {id} not found", nameof(id));
            }

            message.IsSent = isSuccess;
            message.SentOn = isSuccess ? DateTime.UtcNow : null;
            message.ErrorMessage = errorMessage;
            message.ModifiedOn = DateTime.UtcNow;
            message.ModifiedBy = "MessageProcessingService";

            if (!isSuccess)
            {
                message.RetryCount++;
            }

            await _context.SaveChangesAsync();
            return message;
        }

        /// <summary>
        /// Gets the database context associated with this repository
        /// </summary>
        /// <returns>The database context</returns>
        public ApplicationDbContext GetContext()
        {
            return _context;
        }
    }
}
