/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using msih.p4g.Server.Features.Base.EmailService.Interfaces;
using msih.p4g.Server.Features.Base.MessageService.Interfaces;
using msih.p4g.Server.Features.Base.MessageService.Models;
using msih.p4g.Server.Features.Base.MessageService.Utilities;
using msih.p4g.Server.Features.Base.SmsService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace msih.p4g.Server.Features.Base.MessageService.Services
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

            Message message = null;

            try
            {
                // Create message record first if needed
                if (saveToDatabase)
                {
                    message = new Message
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

            Message message = null;

            try
            {
                // Create message record first if needed
                if (saveToDatabase)
                {
                    message = new Message
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

            Message message = null;
            MessageTemplateUsage templateUsage = null;

            try
            {
                // Create message record if needed
                if (saveToDatabase)
                {
                    message = new Message
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
        public async Task<Message> ScheduleEmailAsync(string to, string subject, string htmlContent, DateTime scheduledFor, string from = null)
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("Recipient email cannot be null or empty", nameof(to));

            if (!_emailService.IsValidEmail(to))
                throw new ArgumentException("Invalid recipient email address", nameof(to));

            if (from != null && !_emailService.IsValidEmail(from))
                throw new ArgumentException("Invalid sender email address", nameof(from));

            var message = new Message
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
        public async Task<Message> ScheduleSmsAsync(string to, string content, DateTime scheduledFor, string from = null)
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("Recipient phone number cannot be null or empty", nameof(to));

            if (!_smsService.IsValidPhoneNumber(to))
                throw new ArgumentException("Invalid recipient phone number", nameof(to));

            var message = new Message
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
        public async Task<Message> ScheduleTemplatedMessageAsync(
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
            var message = new Message
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
        public async Task<IEnumerable<Message>> GetMessageHistoryAsync(string recipient, int limit = 50)
        {
            if (string.IsNullOrWhiteSpace(recipient))
                throw new ArgumentException("Recipient cannot be null or empty", nameof(recipient));

            return await _messageRepository.GetMessagesByRecipientAsync(recipient, limit);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Message>> GetRecentEmailsAsync(int limit = 50)
        {
            return await _messageRepository.GetMessagesByTypeAsync(_emailType, limit);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Message>> GetRecentSmsAsync(int limit = 50)
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
    }
}
