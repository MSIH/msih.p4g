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
using MessageEntity = MSIH.Core.Services.Messages.Models.Message;
using MessageTemplate = MSIH.Core.Services.Messages.Models.MessageTemplate;

namespace MSIH.Core.Services.Messages.Interfaces
{
    /// <summary>
    /// Service interface for sending and managing messages (both email and SMS)
    /// </summary>
    public interface IMessageService
    {
        /// <summary>
        /// Sends an email message
        /// </summary>
        /// <param name="to">Recipient email address</param>
        /// <param name="subject">Email subject</param>
        /// <param name="htmlContent">HTML content of the email</param>
        /// <param name="from">Sender email address (optional, uses default if not specified)</param>
        /// <param name="saveToDatabase">Whether to save the message to the database</param>
        /// <returns>True if the email was sent successfully</returns>
        Task<bool> SendEmailAsync(string to, string subject, string htmlContent, string from = null, bool saveToDatabase = true);

        /// <summary>
        /// Sends an SMS message
        /// </summary>
        /// <param name="to">Recipient phone number</param>
        /// <param name="content">SMS content</param>
        /// <param name="from">Sender phone number (optional, uses default if not specified)</param>
        /// <param name="saveToDatabase">Whether to save the message to the database</param>
        /// <returns>True if the SMS was sent successfully</returns>
        Task<bool> SendSmsAsync(string to, string content, string from = null, bool saveToDatabase = true);

        /// <summary>
        /// Sends a message using a template
        /// </summary>
        /// <param name="templateId">The ID of the template to use</param>
        /// <param name="to">Recipient email or phone number</param>
        /// <param name="placeholderValues">Dictionary of placeholder values to substitute in the template</param>
        /// <param name="from">Sender email or phone number (optional, uses template default if not specified)</param>
        /// <param name="subject">Subject override (optional, uses template default if not specified)</param>
        /// <param name="saveToDatabase">Whether to save the message to the database</param>
        /// <returns>True if the message was sent successfully</returns>
        Task<bool> SendTemplatedMessageAsync(
            int templateId,
            string to,
            Dictionary<string, string> placeholderValues,
            string from = null,
            string subject = null,
            bool saveToDatabase = true);

        /// <summary>
        /// Sends a message using a template by name
        /// </summary>
        /// <param name="templateName">The name of the template to use</param>
        /// <param name="to">Recipient email or phone number</param>
        /// <param name="placeholderValues">Dictionary of placeholder values to substitute in the template</param>
        /// <param name="from">Sender email or phone number (optional, uses template default if not specified)</param>
        /// <param name="subject">Subject override (optional, uses template default if not specified)</param>
        /// <param name="saveToDatabase">Whether to save the message to the database</param>
        /// <returns>True if the message was sent successfully</returns>
        Task<bool> SendTemplatedMessageByNameAsync(
            string templateName,
            string to,
            Dictionary<string, string> placeholderValues,
            string from = null,
            string subject = null,
            bool saveToDatabase = true);

        /// <summary>
        /// Schedules a templated message to be sent at a later time
        /// </summary>
        /// <param name="templateId">The ID of the template to use</param>
        /// <param name="to">Recipient email or phone number</param>
        /// <param name="placeholderValues">Dictionary of placeholder values to substitute in the template</param>
        /// <param name="scheduledFor">When to send the message</param>
        /// <param name="from">Sender email or phone number (optional, uses template default if not specified)</param>
        /// <param name="subject">Subject override (optional, uses template default if not specified)</param>
        /// <returns>The created message entity</returns>
        Task<MessageEntity> ScheduleTemplatedMessageAsync(
            int templateId,
            string to,
            Dictionary<string, string> placeholderValues,
            DateTime scheduledFor,
            string from = null,
            string subject = null);

        /// <summary>
        /// Schedules an email to be sent at a later time
        /// </summary>
        /// <param name="to">Recipient email address</param>
        /// <param name="subject">Email subject</param>
        /// <param name="htmlContent">HTML content of the email</param>
        /// <param name="scheduledFor">When to send the email</param>
        /// <param name="from">Sender email address (optional, uses default if not specified)</param>
        /// <returns>The created message entity</returns>
        Task<MessageEntity> ScheduleEmailAsync(string to, string subject, string htmlContent, DateTime scheduledFor, string from = null);

        /// <summary>
        /// Schedules an SMS to be sent at a later time
        /// </summary>
        /// <param name="to">Recipient phone number</param>
        /// <param name="content">SMS content</param>
        /// <param name="scheduledFor">When to send the SMS</param>
        /// <param name="from">Sender phone number (optional, uses default if not specified)</param>
        /// <returns>The created message entity</returns>
        Task<MessageEntity> ScheduleSmsAsync(string to, string content, DateTime scheduledFor, string from = null);

        /// <summary>
        /// Gets message history for a recipient
        /// </summary>
        /// <param name="recipient">Recipient email or phone number</param>
        /// <param name="limit">Maximum number of messages to retrieve</param>
        /// <returns>Collection of messages sent to the recipient</returns>
        Task<IEnumerable<MessageEntity>> GetMessageHistoryAsync(string recipient, int limit = 50);

        /// <summary>
        /// Gets recent email messages
        /// </summary>
        /// <param name="limit">Maximum number of messages to retrieve</param>
        /// <returns>Collection of recent email messages</returns>
        Task<IEnumerable<MessageEntity>> GetRecentEmailsAsync(int limit = 50);

        /// <summary>
        /// Gets recent SMS messages
        /// </summary>
        /// <param name="limit">Maximum number of messages to retrieve</param>
        /// <returns>Collection of recent SMS messages</returns>
        Task<IEnumerable<MessageEntity>> GetRecentSmsAsync(int limit = 50);

        /// <summary>
        /// Processes pending messages that are due to be sent
        /// </summary>
        /// <param name="limit">Maximum number of messages to process</param>
        /// <returns>Number of messages successfully sent</returns>
        Task<int> ProcessPendingMessagesAsync(int limit = 50);

        /// <summary>
        /// Processes scheduled messages that are due to be sent
        /// </summary>
        /// <param name="limit">Maximum number of messages to process</param>
        /// <returns>Number of messages successfully sent</returns>
        Task<int> ProcessScheduledMessagesAsync(int limit = 50);

        /// <summary>
        /// Processes failed messages for retry
        /// </summary>
        /// <param name="limit">Maximum number of messages to process</param>
        /// <returns>Number of messages successfully sent</returns>
        Task<int> ProcessFailedMessagesAsync(int limit = 50);

        /// <summary>
        /// Gets all available message templates
        /// </summary>
        /// <param name="messageType">Optional filter by message type (Email or SMS)</param>
        /// <returns>Collection of message templates</returns>
        Task<IEnumerable<MessageTemplate>> GetAllTemplatesAsync(string messageType = null);

        /// <summary>
        /// Gets templates by category
        /// </summary>
        /// <param name="category">The template category (e.g., ThankYou, DonationSolicitation)</param>
        /// <param name="messageType">Optional message type filter (Email or SMS)</param>
        /// <returns>Collection of templates in the specified category</returns>
        Task<IEnumerable<MessageTemplate>> GetTemplatesByCategoryAsync(string category, string messageType = null);

        /// <summary>
        /// Gets the default template for a category
        /// </summary>
        /// <param name="category">The template category</param>
        /// <param name="messageType">The message type (Email or SMS)</param>
        /// <returns>The default template for the category and message type</returns>
        Task<MessageTemplate> GetDefaultTemplateAsync(string category, string messageType);

        /// <summary>
        /// Gets a template by ID
        /// </summary>
        /// <param name="id">The ID of the template to retrieve</param>
        /// <returns>The template with the specified ID, or null if not found</returns>
        Task<MessageTemplate> GetTemplateByIdAsync(int id);

        /// <summary>
        /// Creates a new message template
        /// </summary>
        /// <param name="template">The template to create</param>
        /// <returns>The created template with ID assigned</returns>
        Task<MessageTemplate> CreateTemplateAsync(MessageTemplate template);

        /// <summary>
        /// Updates an existing message template
        /// </summary>
        /// <param name="template">The template to update</param>
        /// <returns>The updated template</returns>
        Task<MessageTemplate> UpdateTemplateAsync(MessageTemplate template);

        /// <summary>
        /// Deletes a message template
        /// </summary>
        /// <param name="id">The ID of the template to delete</param>
        /// <returns>True if deleted successfully, false otherwise</returns>
        Task<bool> DeleteTemplateAsync(int id);

        /// <summary>
        /// Sets a template as the default for its category and message type
        /// </summary>
        /// <param name="id">The ID of the template to set as default</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> SetTemplateAsDefaultAsync(int id);
    }
}
