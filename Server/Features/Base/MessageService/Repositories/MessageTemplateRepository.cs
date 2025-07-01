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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.Base.MessageService.Repositories
{
    /// <summary>
    /// Repository implementation for MessageTemplate entities
    /// </summary>
    public class MessageTemplateRepository : GenericRepository<MessageTemplate>, IMessageTemplateRepository
    {
        public MessageTemplateRepository(IDbContextFactory<ApplicationDbContext> contextFactory) : base(contextFactory)
        {
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MessageTemplate>> GetByCategoryAsync(string category, string messageType = null)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                throw new ArgumentException("Category cannot be null or empty", nameof(category));
            }

            using var context = await _contextFactory.CreateDbContextAsync();
            var query = context.Set<MessageTemplate>()
                .Where(t => t.Category == category && t.IsActive);

            if (!string.IsNullOrWhiteSpace(messageType))
            {
                query = query.Where(t => t.MessageType == messageType);
            }

            return await query.OrderByDescending(t => t.IsDefault).ThenBy(t => t.Name).ToListAsync();
        }

        /// <inheritdoc />
        public async Task<MessageTemplate> GetDefaultTemplateAsync(string category, string messageType)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                throw new ArgumentException("Category cannot be null or empty", nameof(category));
            }

            if (string.IsNullOrWhiteSpace(messageType))
            {
                throw new ArgumentException("MessageType cannot be null or empty", nameof(messageType));
            }

            using var context = await _contextFactory.CreateDbContextAsync();

            // Try to find a template marked as default
            var defaultTemplate = await context.Set<MessageTemplate>()
                .Where(t => t.Category == category && 
                           t.MessageType == messageType && 
                           t.IsDefault && 
                           t.IsActive)
                .FirstOrDefaultAsync();

            // If no default is found, return the first active template in the category
            if (defaultTemplate == null)
            {
                defaultTemplate = await context.Set<MessageTemplate>()
                    .Where(t => t.Category == category && 
                               t.MessageType == messageType && 
                               t.IsActive)
                    .OrderBy(t => t.CreatedOn)
                    .FirstOrDefaultAsync();
            }

            return defaultTemplate;
        }

        /// <inheritdoc />
        public async Task<MessageTemplate> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name cannot be null or empty", nameof(name));
            }

            using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Set<MessageTemplate>()
                .Where(t => t.Name == name && t.IsActive)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc />
        public async Task<bool> SetAsDefaultAsync(int templateId)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var template = await context.Set<MessageTemplate>().FindAsync(templateId);
            if (template == null || !template.IsActive)
            {
                return false;
            }

            // Begin transaction to ensure consistency
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // Clear default flag from all templates in the same category and message type
                var templates = await context.Set<MessageTemplate>()
                    .Where(t => t.Category == template.Category && 
                               t.MessageType == template.MessageType && 
                               t.IsDefault && 
                               t.IsActive)
                    .ToListAsync();

                foreach (var existingDefault in templates)
                {
                    existingDefault.IsDefault = false;
                    existingDefault.ModifiedOn = DateTime.UtcNow;
                    existingDefault.ModifiedBy = "System";
                }

                // Set the new template as default
                template.IsDefault = true;
                template.ModifiedOn = DateTime.UtcNow;
                template.ModifiedBy = "System";

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
