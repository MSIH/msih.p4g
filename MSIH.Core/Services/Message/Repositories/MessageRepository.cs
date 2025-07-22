/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;
using MSIH.Core.Common.Data;
using MSIH.Core.Common.Data.Repositories;
using MSIH.Core.Services.Message.Interfaces;
using MessageEntity = MSIH.Core.Services.Message.Models.Message;

namespace MSIH.Core.Services.Message.Repositories
{
    /// <summary>
    /// Repository implementation for Message entities
    /// </summary>
    public class MessageRepository : GenericRepository<MessageEntity>, IMessageRepository
    {
        public MessageRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
        {
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MessageEntity>> GetPendingMessagesAsync(int limit = 50, int maxRetries = 3)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<MessageEntity>()
                .Where(m => !m.IsSent && m.IsActive &&
                          m.RetryCount < maxRetries &&
                          (m.ScheduledFor == null || m.ScheduledFor <= DateTime.UtcNow))
                .OrderBy(m => m.CreatedOn)
                .Take(limit)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MessageEntity>> GetScheduledMessagesAsync(DateTime before, int limit = 50, int maxRetries = 3)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<MessageEntity>()
                .Where(m => !m.IsSent && m.IsActive &&
                          m.ScheduledFor != null && m.ScheduledFor <= before &&
                          m.RetryCount < maxRetries)
                .OrderBy(m => m.ScheduledFor)
                .Take(limit)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MessageEntity>> GetFailedMessagesAsync(int limit = 50, int maxRetries = 3)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<MessageEntity>()
                .Where(m => !m.IsSent && m.IsActive &&
                          m.RetryCount > 0 && m.RetryCount < maxRetries &&
                          !string.IsNullOrEmpty(m.ErrorMessage))
                .OrderBy(m => m.ModifiedOn ?? m.CreatedOn)
                .Take(limit)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MessageEntity>> GetMessagesByTypeAsync(string messageType, int limit = 50)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<MessageEntity>()
                .Where(m => m.MessageType == messageType && m.IsActive)
                .OrderByDescending(m => m.CreatedOn)
                .Take(limit)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MessageEntity>> GetMessagesByRecipientAsync(string recipient, int limit = 50)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<MessageEntity>()
                .Where(m => m.To == recipient && m.IsActive)
                .OrderByDescending(m => m.CreatedOn)
                .Take(limit)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<MessageEntity> UpdateMessageStatusAsync(int id, bool isSuccess, string errorMessage = "")
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var message = await context.Set<MessageEntity>().FindAsync(id);
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

            await context.SaveChangesAsync();
            return message;
        }

        /// <summary>
        /// Gets a new database context instance
        /// </summary>
        /// <returns>A new database context instance</returns>
        public async Task<ApplicationDbContext> GetContextAsync()
        {
            return await _contextFactory.CreateDbContextAsync();
        }

        /// <summary>
        /// Gets a new database context instance - Note: Caller is responsible for disposing
        /// </summary>
        /// <returns>A new database context instance</returns>
        public ApplicationDbContext GetContext()
        {
            // This is a legacy method that should be avoided in new code
            // Using CreateDbContextAsync is preferred, but this maintains compatibility
            return _contextFactory.CreateDbContext();
        }
    }
}
