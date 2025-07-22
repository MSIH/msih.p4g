/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using MSIH.Core.Common.Data.Repositories;
using MSIH.Core.Services.Message.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MSIH.Core.Services.Message.Interfaces
{
    /// <summary>
    /// Repository interface for MessageTemplate entities
    /// </summary>
    public interface IMessageTemplateRepository : IGenericRepository<MessageTemplate>
    {
        /// <summary>
        /// Gets templates by category
        /// </summary>
        /// <param name="category">The template category (e.g., ThankYou, DonationSolicitation)</param>
        /// <param name="messageType">Optional message type filter (Email or SMS)</param>
        /// <returns>Collection of templates in the specified category</returns>
        Task<IEnumerable<MessageTemplate>> GetByCategoryAsync(string category, string messageType = null);

        /// <summary>
        /// Gets the default template for a category
        /// </summary>
        /// <param name="category">The template category</param>
        /// <param name="messageType">The message type (Email or SMS)</param>
        /// <returns>The default template for the category and message type</returns>
        Task<MessageTemplate> GetDefaultTemplateAsync(string category, string messageType);

        /// <summary>
        /// Gets a template by name
        /// </summary>
        /// <param name="name">The template name</param>
        /// <returns>The template with the specified name</returns>
        Task<MessageTemplate> GetByNameAsync(string name);

        /// <summary>
        /// Sets a template as the default for its category and message type
        /// </summary>
        /// <param name="templateId">The ID of the template to set as default</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> SetAsDefaultAsync(int templateId);
    }
}
