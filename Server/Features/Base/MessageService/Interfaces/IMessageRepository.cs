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
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.Base.MessageService.Models;

namespace msih.p4g.Server.Features.Base.MessageService.Interfaces
{
    /// <summary>
    /// Repository interface for Message entities
    /// </summary>
    public interface IMessageRepository : IGenericRepository<Message>
    {
        /// <summary>
        /// Gets pending messages that need to be sent
        /// </summary>
        /// <param name="limit">Maximum number of messages to retrieve</param>
        /// <param name="maxRetries">Maximum retry count for messages</param>
        /// <returns>A collection of pending messages</returns>
        Task<IEnumerable<Message>> GetPendingMessagesAsync(int limit = 50, int maxRetries = 3);

        /// <summary>
        /// Gets messages due to be sent at or before the specified time
        /// </summary>
        /// <param name="before">The time threshold</param>
        /// <param name="limit">Maximum number of messages to retrieve</param>
        /// <param name="maxRetries">Maximum retry count for messages</param>
        /// <returns>A collection of scheduled messages due for sending</returns>
        Task<IEnumerable<Message>> GetScheduledMessagesAsync(DateTime before, int limit = 50, int maxRetries = 3);

        /// <summary>
        /// Gets failed messages that are eligible for retry
        /// </summary>
        /// <param name="limit">Maximum number of messages to retrieve</param>
        /// <param name="maxRetries">Maximum retry count for messages</param>
        /// <returns>A collection of failed messages eligible for retry</returns>
        Task<IEnumerable<Message>> GetFailedMessagesAsync(int limit = 50, int maxRetries = 3);

        /// <summary>
        /// Gets messages by type (Email or SMS)
        /// </summary>
        /// <param name="messageType">The type of message</param>
        /// <param name="limit">Maximum number of messages to retrieve</param>
        /// <returns>A collection of messages of the specified type</returns>
        Task<IEnumerable<Message>> GetMessagesByTypeAsync(string messageType, int limit = 50);

        /// <summary>
        /// Gets messages sent to a specific recipient
        /// </summary>
        /// <param name="recipient">The recipient's email or phone number</param>
        /// <param name="limit">Maximum number of messages to retrieve</param>
        /// <returns>A collection of messages sent to the recipient</returns>
        Task<IEnumerable<Message>> GetMessagesByRecipientAsync(string recipient, int limit = 50);

        /// <summary>
        /// Updates the message status after a send attempt
        /// </summary>
        /// <param name="id">The message ID</param>
        /// <param name="isSuccess">Whether the send was successful</param>
        /// <param name="errorMessage">Any error message, if applicable</param>
        /// <returns>The updated message</returns>
        Task<Message> UpdateMessageStatusAsync(int id, bool isSuccess, string errorMessage = "");

        /// <summary>
        /// Gets the database context associated with this repository
        /// </summary>
        /// <returns>The database context</returns>
        ApplicationDbContext GetContext();
    }
}
