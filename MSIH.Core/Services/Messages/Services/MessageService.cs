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
using Microsoft.Extensions.Logging;
using MSIH.Core.Services.Email.Interfaces;
using MSIH.Core.Services.Messages.Interfaces;
using MSIH.Core.Services.Messages.Utilities;
using MSIH.Core.Services.Sms.Interfaces;
using MessageEntity = MSIH.Core.Services.Messages.Models.Message;
using MessageTemplate = MSIH.Core.Services.Messages.Models.MessageTemplate;
using MessageTemplateUsage = MSIH.Core.Services.Messages.Models.MessageTemplateUsage;

namespace MSIH.Core.Services.Messages.Services
{
    /// <summary>
    /// Implementation of the message service that handles both email and SMS messages
    /// </summary>
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IMessageTemplateRepository _templateRepository;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ILogger<MessageService> _logger;

        private const string _emailType = "Email";
        private const string _smsType = "SMS";

        public MessageService(
            IMessageRepository messageRepository,
            IMessageTemplateRepository templateRepository,
            IEmailService emailService,
            ISmsService smsService,
            ILogger<MessageService> logger)
        {
            _messageRepository = messageRepository ?? throw new ArgumentNullException(nameof(messageRepository));
            _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> SendEmailAsync(string to, string subject, string htmlContent, string from = null, bool saveToDatabase = true)
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("Recipient email cannot be null or empty", nameof(to));

            if (!_emailService.IsValidEmail(to))
                throw new ArgumentException("Invalid recipient email address", nameof(to));

            if (from != null && !_emailService.IsValidEmail(from))
                throw new ArgumentException("Invalid sender email address", nameof(from));

            MessageEntity message = null;

            try
            {
                // Create message record first if needed
                if (saveToDatabase)
                {
                    message = new MessageEntity
                    {
                        MessageType = _emailType,
                        From = from ?? string.Empty, // Will use default in the email service if empty
                        To = to,
                        Subject = subject,
                        Content = htmlContent,
                        IsHtml = true,
                        CreatedBy = "MessageService"
                    };

                    message = await _messageRepository.AddAsync(message);
                }

                // Send the email
                await _emailService.SendEmailAsync(to, from, subject, htmlContent);

                // Update the message record if it exists
                if (message != null)
                {
                    await _messageRepository.UpdateMessageStatusAsync(message.Id, true);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Recipient}", to);

                // Update the message record with the error if it exists
                if (message != null)
                {
                    await _messageRepository.UpdateMessageStatusAsync(message.Id, false, ex.Message);
                }

                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> SendSmsAsync(string to, string content, string from = null, bool saveToDatabase = true)
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("Recipient phone number cannot be null or empty", nameof(to));

            if (!_smsService.IsValidPhoneNumber(to))
                throw new ArgumentException("Invalid recipient phone number", nameof(to));

            MessageEntity message = null;

            try
            {
                // Create message record first if needed
                if (saveToDatabase)
                {
                    message = new MessageEntity
                    {
                        MessageType = _smsType,
                        From = from ?? string.Empty, // Will use default in the SMS service if empty
                        To = to,
                        Content = content,
                        IsHtml = false,
                        CreatedBy = "MessageService"
                    };

                    message = await _messageRepository.AddAsync(message);
                }

                // Send the SMS
                await _smsService.SendSmsAsync(to, content);

                // Update the message record if it exists
                if (message != null)
                {
                    await _messageRepository.UpdateMessageStatusAsync(message.Id, true);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS to {Recipient}", to);

                // Update the message record with the error if it exists
                if (message != null)
                {
                    await _messageRepository.UpdateMessageStatusAsync(message.Id, false, ex.Message);
                }

                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> SendTemplatedMessageAsync(
            int templateId,
            string to,
            Dictionary<string, string> placeholderValues,
            string from = null,
            string subject = null,
            bool saveToDatabase = true)
        {
            var template = await _templateRepository.GetByIdAsync(templateId);
            if (template == null)
            {
                throw new ArgumentException($"Template with ID {templateId} not found", nameof(templateId));
            }

            return await SendTemplatedMessageInternalAsync(template, to, placeholderValues, from, subject, saveToDatabase);
        }

        /// <inheritdoc />
        public async Task<bool> SendTemplatedMessageByNameAsync(
            string templateName,
            string to,
            Dictionary<string, string> placeholderValues,
            string from = null,
            string subject = null,
            bool saveToDatabase = true)
        {
            var template = await _templateRepository.GetByNameAsync(templateName);
            if (template == null)
            {
                throw new ArgumentException($"Template with name '{templateName}' not found", nameof(templateName));
            }

            return await SendTemplatedMessageInternalAsync(template, to, placeholderValues, from, subject, saveToDatabase);
        }

        /// <summary>
        /// Internal method for sending templated messages
        /// </summary>
        private async Task<bool> SendTemplatedMessageInternalAsync(
            MessageTemplate template,
            string to,
            Dictionary<string, string> placeholderValues,
            string from = null,
            string subject = null,
            bool saveToDatabase = true)
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("Recipient cannot be null or empty", nameof(to));

            if (placeholderValues == null)
                placeholderValues = new Dictionary<string, string>();

            // Validate that all required placeholders have values
            if (!TemplateProcessor.ValidatePlaceholders(template.TemplateContent, placeholderValues, out var missingPlaceholders) && missingPlaceholders.Count > 0)
            {
                throw new ArgumentException($"Missing required placeholders: {string.Join(", ", missingPlaceholders)}", nameof(placeholderValues));
            }

            // Process the template content
            string processedContent = TemplateProcessor.ProcessTemplate(template.TemplateContent, placeholderValues);

            // Use the template's default sender if none provided
            string effectiveFrom = from ?? template.DefaultSender;

            // Use the provided subject or template default for emails
            string effectiveSubject = template.MessageType == _emailType
                ? (subject ?? template.DefaultSubject)
                : null;

            MessageEntity message = null;
            MessageTemplateUsage templateUsage = null;

            try
            {
                // Create message record if needed
                if (saveToDatabase)
                {
                    message = new MessageEntity
                    {
                        MessageType = template.MessageType,
                        From = effectiveFrom,
                        To = to,
                        Subject = effectiveSubject ?? string.Empty,
                        Content = processedContent,
                        IsHtml = template.IsHtml,
                        CreatedBy = "MessageService"
                    };

                    message = await _messageRepository.AddAsync(message);

                    // Create template usage record
                    templateUsage = new MessageTemplateUsage
                    {
                        MessageId = message.Id,
                        TemplateId = template.Id,
                        PlaceholderValues = placeholderValues
                    };

                    await _messageRepository.GetContext().Set<MessageTemplateUsage>().AddAsync(templateUsage);
                    await _messageRepository.GetContext().SaveChangesAsync();
                }

                bool success;
                // Send the message based on type
                if (template.MessageType == _emailType)
                {
                    success = await SendEmailAsync(to, effectiveSubject ?? string.Empty, processedContent, effectiveFrom, false);
                }
                else if (template.MessageType == _smsType)
                {
                    success = await SendSmsAsync(to, processedContent, effectiveFrom, false);
                }
                else
                {
                    throw new NotSupportedException($"Message type '{template.MessageType}' is not supported");
                }

                // Update the message status
                if (message != null)
                {
                    await _messageRepository.UpdateMessageStatusAsync(message.Id, success);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send templated message to {Recipient} using template {TemplateId}", to, template.Id);

                // Update the message status if it exists
                if (message != null)
                {
                    await _messageRepository.UpdateMessageStatusAsync(message.Id, false, ex.Message);
                }

                return false;
            }
        }

        /// <inheritdoc />
        public async Task<MessageEntity> ScheduleEmailAsync(string to, string subject, string htmlContent, DateTime scheduledFor, string from = null)
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("Recipient email cannot be null or empty", nameof(to));

            if (!_emailService.IsValidEmail(to))
                throw new ArgumentException("Invalid recipient email address", nameof(to));

            if (from != null && !_emailService.IsValidEmail(from))
                throw new ArgumentException("Invalid sender email address", nameof(from));

            var message = new MessageEntity
            {
                MessageType = _emailType,
                From = from ?? string.Empty,
                To = to,
                Subject = subject,
                Content = htmlContent,
                IsHtml = true,
                ScheduledFor = scheduledFor,
                CreatedBy = "MessageService"
            };

            return await _messageRepository.AddAsync(message);
        }

        /// <inheritdoc />
        public async Task<MessageEntity> ScheduleSmsAsync(string to, string content, DateTime scheduledFor, string from = null)
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("Recipient phone number cannot be null or empty", nameof(to));

            if (!_smsService.IsValidPhoneNumber(to))
                throw new ArgumentException("Invalid recipient phone number", nameof(to));

            var message = new MessageEntity
            {
                MessageType = _smsType,
                From = from ?? string.Empty,
                To = to,
                Content = content,
                IsHtml = false,
                ScheduledFor = scheduledFor,
                CreatedBy = "MessageService"
            };

            return await _messageRepository.AddAsync(message);
        }

        /// <inheritdoc />
        public async Task<MessageEntity> ScheduleTemplatedMessageAsync(
            int templateId,
            string to,
            Dictionary<string, string> placeholderValues,
            DateTime scheduledFor,
            string from = null,
            string subject = null)
        {
            var template = await _templateRepository.GetByIdAsync(templateId);
            if (template == null)
            {
                throw new ArgumentException($"Template with ID {templateId} not found", nameof(templateId));
            }

            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("Recipient cannot be null or empty", nameof(to));

            if (placeholderValues == null)
                placeholderValues = new Dictionary<string, string>();

            // Validate that all required placeholders have values
            if (!TemplateProcessor.ValidatePlaceholders(template.TemplateContent, placeholderValues, out var missingPlaceholders) && missingPlaceholders.Count > 0)
            {
                throw new ArgumentException($"Missing required placeholders: {string.Join(", ", missingPlaceholders)}", nameof(placeholderValues));
            }

            // Use the template's default sender if none provided
            string effectiveFrom = from ?? template.DefaultSender;

            // Use the provided subject or template default for emails
            string effectiveSubject = template.MessageType == _emailType
                ? (subject ?? template.DefaultSubject)
                : null;

            // Create the message record
            var message = new MessageEntity
            {
                MessageType = template.MessageType,
                From = effectiveFrom,
                To = to,
                Subject = effectiveSubject ?? string.Empty,
                Content = template.TemplateContent, // Store the original template content
                IsHtml = template.IsHtml,
                ScheduledFor = scheduledFor,
                CreatedBy = "MessageService"
            };

            message = await _messageRepository.AddAsync(message);

            // Create template usage record with placeholder values
            var templateUsage = new MessageTemplateUsage
            {
                MessageId = message.Id,
                TemplateId = template.Id,
                PlaceholderValues = placeholderValues
            };

            await _messageRepository.GetContext().Set<MessageTemplateUsage>().AddAsync(templateUsage);
            await _messageRepository.GetContext().SaveChangesAsync();

            return message;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MessageEntity>> GetMessageHistoryAsync(string recipient, int limit = 50)
        {
            if (string.IsNullOrWhiteSpace(recipient))
                throw new ArgumentException("Recipient cannot be null or empty", nameof(recipient));

            return await _messageRepository.GetMessagesByRecipientAsync(recipient, limit);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MessageEntity>> GetRecentEmailsAsync(int limit = 50)
        {
            return await _messageRepository.GetMessagesByTypeAsync(_emailType, limit);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MessageEntity>> GetRecentSmsAsync(int limit = 50)
        {
            return await _messageRepository.GetMessagesByTypeAsync(_smsType, limit);
        }

        /// <inheritdoc />
        public async Task<int> ProcessPendingMessagesAsync(int limit = 50)
        {
            try
            {
                // Get pending messages that are due to be sent
                var pendingMessages = await _messageRepository.GetPendingMessagesAsync(limit);
                int successCount = 0;

                foreach (var message in pendingMessages)
                {
                    try
                    {
                        bool success = await ProcessSingleMessageAsync(message);
                        if (success)
                        {
                            successCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message ID {MessageId}", message.Id);
                        await _messageRepository.UpdateMessageStatusAsync(message.Id, false, ex.Message);
                    }
                }

                return successCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing pending messages");
                return 0;
            }
        }

        /// <inheritdoc />
        public async Task<int> ProcessScheduledMessagesAsync(int limit = 50)
        {
            try
            {
                // Get scheduled messages that are due to be sent
                var scheduledMessages = await _messageRepository.GetScheduledMessagesAsync(DateTime.UtcNow, limit);
                int successCount = 0;

                foreach (var message in scheduledMessages)
                {
                    try
                    {
                        bool success = await ProcessSingleMessageAsync(message);
                        if (success)
                        {
                            successCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing scheduled message ID {MessageId}", message.Id);
                        await _messageRepository.UpdateMessageStatusAsync(message.Id, false, ex.Message);
                    }
                }

                return successCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing scheduled messages");
                return 0;
            }
        }

        /// <inheritdoc />
        public async Task<int> ProcessFailedMessagesAsync(int limit = 50)
        {
            try
            {
                // Get failed messages that are eligible for retry
                var failedMessages = await _messageRepository.GetFailedMessagesAsync(limit);
                int successCount = 0;

                foreach (var message in failedMessages)
                {
                    try
                    {
                        _logger.LogInformation("Retrying failed message ID {MessageId} (Attempt {RetryCount})", message.Id, message.RetryCount + 1);

                        bool success = await ProcessSingleMessageAsync(message);
                        if (success)
                        {
                            successCount++;
                            _logger.LogInformation("Successfully retried message ID {MessageId}", message.Id);
                        }
                        else
                        {
                            _logger.LogWarning("Retry failed for message ID {MessageId}", message.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error retrying failed message ID {MessageId}", message.Id);
                        await _messageRepository.UpdateMessageStatusAsync(message.Id, false, ex.Message);
                    }
                }

                return successCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing failed messages");
                return 0;
            }
        }

        /// <summary>
        /// Processes a single message (handles both regular and templated messages)
        /// </summary>
        private async Task<bool> ProcessSingleMessageAsync(MessageEntity message)
        {
            bool success = false;

            // Check if this is a templated message
            var templateUsage = await _messageRepository.GetContext().Set<MessageTemplateUsage>()
                .FirstOrDefaultAsync(tu => tu.MessageId == message.Id);

            if (templateUsage != null)
            {
                // This is a templated message, retrieve the template and process it
                var template = await _templateRepository.GetByIdAsync(templateUsage.TemplateId);
                if (template != null)
                {
                    // Process the template content with the saved placeholder values
                    string processedContent = TemplateProcessor.ProcessTemplate(
                        template.TemplateContent,
                        templateUsage.PlaceholderValues);

                    // Send based on message type
                    if (message.MessageType == _emailType)
                    {
                        success = await SendEmailAsync(
                            message.To,
                            message.Subject,
                            processedContent,
                            message.From,
                            false);
                    }
                    else if (message.MessageType == _smsType)
                    {
                        success = await SendSmsAsync(
                            message.To,
                            processedContent,
                            message.From,
                            false);
                    }
                }
            }
            else
            {
                // This is a regular message, send it directly
                if (message.MessageType == _emailType)
                {
                    success = await SendEmailAsync(
                        message.To,
                        message.Subject,
                        message.Content,
                        message.From,
                        false);
                }
                else if (message.MessageType == _smsType)
                {
                    success = await SendSmsAsync(
                        message.To,
                        message.Content,
                        message.From,
                        false);
                }
            }

            // Update the message status
            await _messageRepository.UpdateMessageStatusAsync(
                message.Id,
                success,
                success ? null : "Failed to send message");

            return success;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MessageTemplate>> GetAllTemplatesAsync(string messageType = null)
        {
            if (string.IsNullOrWhiteSpace(messageType))
            {
                return await _templateRepository.GetAllAsync();
            }
            else
            {
                return await _templateRepository.FindAsync(t => t.MessageType == messageType);
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<MessageTemplate>> GetTemplatesByCategoryAsync(string category, string messageType = null)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category cannot be null or empty", nameof(category));

            return await _templateRepository.GetByCategoryAsync(category, messageType);
        }

        /// <inheritdoc />
        public async Task<MessageTemplate> GetDefaultTemplateAsync(string category, string messageType)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category cannot be null or empty", nameof(category));

            if (string.IsNullOrWhiteSpace(messageType))
                throw new ArgumentException("MessageType cannot be null or empty", nameof(messageType));

            return await _templateRepository.GetDefaultTemplateAsync(category, messageType);
        }

        /// <inheritdoc />
        public async Task<MessageTemplate> GetTemplateByIdAsync(int id)
        {
            return await _templateRepository.GetByIdAsync(id);
        }

        /// <inheritdoc />
        public async Task<MessageTemplate> CreateTemplateAsync(MessageTemplate template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            // Validate the template
            ValidateTemplate(template);

            // If this is set as default, we need to unset any existing defaults
            if (template.IsDefault)
            {
                await UnsetDefaultTemplatesAsync(template.Category, template.MessageType);
            }

            // Set audit properties
            template.CreatedOn = DateTime.UtcNow;
            template.IsActive = true;
            template.CreatedBy = template.CreatedBy ?? "SettingService";

            // Create the template
            return await _templateRepository.AddAsync(template);
        }

        /// <inheritdoc />
        public async Task<MessageTemplate> UpdateTemplateAsync(MessageTemplate template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            // Validate the template
            ValidateTemplate(template);

            // Get the existing template
            var existingTemplate = await _templateRepository.GetByIdAsync(template.Id);
            if (existingTemplate == null)
                throw new ArgumentException($"Template with ID {template.Id} not found", nameof(template.Id));

            // If this is set as default, we need to unset any existing defaults
            if (template.IsDefault && !existingTemplate.IsDefault)
            {
                await UnsetDefaultTemplatesAsync(template.Category, template.MessageType);
            }

            // Update the template
            existingTemplate.Name = template.Name;
            existingTemplate.Description = template.Description;
            existingTemplate.Category = template.Category;
            existingTemplate.MessageType = template.MessageType;
            existingTemplate.DefaultSubject = template.DefaultSubject;
            existingTemplate.DefaultSender = template.DefaultSender;
            existingTemplate.TemplateContent = template.TemplateContent;
            existingTemplate.IsHtml = template.IsHtml;
            existingTemplate.AvailablePlaceholders = template.AvailablePlaceholders;
            existingTemplate.IsDefault = template.IsDefault;
            existingTemplate.ModifiedOn = DateTime.UtcNow;
            existingTemplate.ModifiedBy = template.ModifiedBy ?? "MessageService";

            await _templateRepository.UpdateAsync(existingTemplate);
            return existingTemplate;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteTemplateAsync(int id)
        {
            var template = await _templateRepository.GetByIdAsync(id);
            if (template == null)
                return false;

            // Check if there are any messages using this template
            var usageCount = await _messageRepository.GetContext().Set<MessageTemplateUsage>()
                .CountAsync(tu => tu.TemplateId == id);

            if (usageCount > 0)
            {
                // Soft delete if there are messages using this template
                template.IsActive = false;
                template.ModifiedOn = DateTime.UtcNow;
                template.ModifiedBy = "MessageService";
                await _templateRepository.UpdateAsync(template);
            }
            else
            {
                // Also soft delete for consistency
                template.IsActive = false;
                template.ModifiedOn = DateTime.UtcNow;
                template.ModifiedBy = "MessageService";
                await _templateRepository.UpdateAsync(template);
            }

            return true;
        }

        /// <inheritdoc />
        public async Task<bool> SetTemplateAsDefaultAsync(int id)
        {
            return await _templateRepository.SetAsDefaultAsync(id);
        }

        /// <summary>
        /// Validates a message template
        /// </summary>
        private void ValidateTemplate(MessageTemplate template)
        {
            if (string.IsNullOrWhiteSpace(template.Name))
                throw new ArgumentException("Template name cannot be empty", nameof(template.Name));

            if (string.IsNullOrWhiteSpace(template.Category))
                throw new ArgumentException("Template category cannot be empty", nameof(template.Category));

            if (string.IsNullOrWhiteSpace(template.MessageType))
                throw new ArgumentException("Template message type cannot be empty", nameof(template.MessageType));

            if (string.IsNullOrWhiteSpace(template.TemplateContent))
                throw new ArgumentException("Template content cannot be empty", nameof(template.TemplateContent));

            if (template.MessageType != _emailType && template.MessageType != _smsType)
                throw new ArgumentException($"Invalid message type: {template.MessageType}. Must be '{_emailType}' or '{_smsType}'", nameof(template.MessageType));

            // SMS templates cannot be HTML
            if (template.MessageType == _smsType && template.IsHtml)
                throw new ArgumentException("SMS templates cannot be HTML", nameof(template.IsHtml));
        }

        /// <summary>
        /// Unsets default flag for all templates in a category and message type
        /// </summary>
        private async Task UnsetDefaultTemplatesAsync(string category, string messageType)
        {
            var templates = await _templateRepository.GetByCategoryAsync(category, messageType);
            foreach (var template in templates.Where(t => t.IsDefault))
            {
                template.IsDefault = false;
                template.ModifiedOn = DateTime.UtcNow;
                template.ModifiedBy = "MessageService";
                await _templateRepository.UpdateAsync(template);
            }
        }
    }
}
