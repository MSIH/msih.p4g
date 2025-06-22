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
using msih.p4g.Server.Features.Base.MessageService.Interfaces;
using msih.p4g.Server.Features.Base.SettingsService.Interfaces;
using msih.p4g.Server.Features.Base.UserService.Interfaces;
using msih.p4g.Server.Features.Base.UserService.Models;
using msih.p4g.Server.Features.Base.UserService.Utilities;

namespace msih.p4g.Server.Features.Base.UserService.Services
{
    public class EmailVerificationService : IEmailVerificationService
    {
        private readonly IUserService _userService;
        private readonly IMessageService _messageService;
        private readonly ISettingsService _settingsService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailVerificationService> _logger;

        private const string EMAIL_VERIFICATION_TEMPLATE = "EmailVerification";
        private const string EMAIL_VERIFICATION_SECRET_KEY = "EmailVerification:SecretKey";
        private const string BASE_URL_SETTING = "BaseUrl";

        public EmailVerificationService(
            IUserService userService,
            IMessageService messageService,
            ISettingsService settingsService,
            IConfiguration configuration,
            ILogger<EmailVerificationService> logger)
        {
            _userService = userService;
            _messageService = messageService;
            _settingsService = settingsService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendVerificationEmailAsync(User user)
        {
            try
            {
                if (user == null)
                    throw new ArgumentNullException(nameof(user));

                // Get the secret key for token generation
                var secretKey = await _settingsService.GetValueAsync(EMAIL_VERIFICATION_SECRET_KEY)
                    ?? _configuration["EmailVerification:SecretKey"]
                    ?? throw new InvalidOperationException("Email verification secret key not configured");

                // Get the base URL for the verification link
                var baseUrl = await _settingsService.GetValueAsync(BASE_URL_SETTING)
                    ?? _configuration["BaseUrl"]
                    ?? "https://localhost:7265";

                // Generate a verification token
                var token = user.GenerateEmailVerificationToken(secretKey);

                // Create the verification link
                var verificationLink = $"{baseUrl}/verify-email?token={token}";

                // Set up email placeholders
                var placeholders = new Dictionary<string, string>
                {
                    { "UserName", user.Email },
                    { "VerificationLink", verificationLink }
                };

                // Send the verification email using the template
                var emailSent = await _messageService.SendTemplatedMessageByNameAsync(
                    EMAIL_VERIFICATION_TEMPLATE,
                    user.Email,
                    placeholders,
                    subject: "Verify Your Email Address"
                );

                if (emailSent)
                {
                    // Update the last verification sent timestamp
                    user.LastEmailVerificationSentAt = DateTime.UtcNow;
                    await _userService.UpdateAsync(user);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending verification email to user {UserId}", user?.Id);
                return false;
            }
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            try
            {
                // Get the secret key for token validation
                var secretKey = await _settingsService.GetValueAsync(EMAIL_VERIFICATION_SECRET_KEY)
                    ?? _configuration["EmailVerification:SecretKey"]
                    ?? throw new InvalidOperationException("Email verification secret key not configured");

                // Validate the token
                var (isValid, userId) = UserExtensions.ValidateEmailVerificationToken(token, secretKey);

                if (isValid && userId.HasValue)
                {
                    // Get the user
                    var user = await _userService.GetByIdAsync(userId.Value);

                    if (user != null)
                    {
                        // Mark the email as verified
                        user.EmailConfirmed = true;
                        user.EmailConfirmedAt = DateTime.UtcNow;

                        // Update the user
                        await _userService.UpdateAsync(user);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email with token");
                return false;
            }
        }

        public async Task<bool> IsEmailVerifiedAsync(int userId)
        {
            try
            {
                var user = await _userService.GetByIdAsync(userId);
                return user?.EmailConfirmed ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email verification for user {UserId}", userId);
                return false;
            }
        }
    }
}
