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
    public class MessageRepository : GenericRepository<Message>, IMessageRepository
    {
        public MessageRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
        {
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Message>> GetPendingMessagesAsync(int limit = 50, int maxRetries = 3)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<Message>()
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
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<Message>()
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
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<Message>()
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
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<Message>()
                .Where(m => m.MessageType == messageType && m.IsActive)
                .OrderByDescending(m => m.CreatedOn)
                .Take(limit)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Message>> GetMessagesByRecipientAsync(string recipient, int limit = 50)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<Message>()
                .Where(m => m.To == recipient && m.IsActive)
                .OrderByDescending(m => m.CreatedOn)
                .Take(limit)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<Message> UpdateMessageStatusAsync(int id, bool isSuccess, string errorMessage = "")
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var message = await context.Set<Message>().FindAsync(id);
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
