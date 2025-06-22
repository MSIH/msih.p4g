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
using Server.Common.Utilities;

namespace msih.p4g.Server.Features.Base.UserService.Services
{
    public class EmailVerificationService : IEmailVerificationService
    {
        private readonly IUserService _userService;
        private readonly IMessageService _messageService;
        private readonly ISettingsService _settingsService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailVerificationService> _logger;

        private const string _eMAIL_VERIFICATION_TEMPLATE = "Email Verification";
        private const string _eMAIL_VERIFICATION_SECRET_KEY = "EmailVerification:SecretKey";
        private const string _bASE_URL_SETTING = "EmailVerification:BaseUrl";

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

                // Get the base URL for the verification link
                var baseUrl = await _settingsService.GetValueAsync(_bASE_URL_SETTING)
                    ?? _configuration["BaseUrl"]
                    ?? "https://localhost:7265";

                // based on the current time get number in format of HHmmss
                var currentTime = DateTime.UtcNow.ToString("HHmmss");
                // covert to int
                var currentTimeInt = int.Parse(currentTime);

                // Generate a verification token
                var token = RandomStringGenerator.Generate(user.Id + currentTimeInt, 8, RandomStringGenerator.CharSet.All);

                // Create the verification link
                var verificationLink = $"{baseUrl}/verify-email?token={token}";

                // Set up email placeholders
                var placeholders = new Dictionary<string, string>
                {
                    { "UserName", user.Email },
                    { "VerificationLink", verificationLink }
                };
                var TemplateContent = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <title>Email Verification</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }
        h1 { color: #2c5282; }
    </style>
</head>
<body>
    <h1>Email Verification Required</h1>
    <p>Dear {{userName}},</p>
    <p>Please click the link below to verify your email address:</p>
    <p><a href=""{{verificationUrl}}"">{{verificationUrl}}</a></p>
    <p>If you did not request this email, please ignore it.</p>
</body>
</html>";

                // Replace placeholders in the template content
                TemplateContent = TemplateContent
                    .Replace("{{userName}}", placeholders["UserName"])
                    .Replace("{{verificationUrl}}", placeholders["VerificationLink"]);

                var emailSent = await _messageService.SendEmailAsync(user.Email, "Verify Email Address", TemplateContent, "donation@msih.org");
                // Send the verification email using the template

                if (!emailSent)
                {
                    _logger.LogWarning("Email verification email failed to send to user {UserId}", user.Id);
                    return false;
                }

                //var emailSent = await _messageService.SendTemplatedMessageByNameAsync(
                //    _eMAIL_VERIFICATION_TEMPLATE,
                //    user.Email,
                //    placeholders,
                //    subject: "Verify Your Email Address"
                //);

                if (emailSent)
                {
                    // Update the last verification sent timestamp
                    user.EmailVerificationToken = token;
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

                // get user by token
                var user = await _userService.GetUserByTokenAsync(token);
                if (user == null)
                {
                    _logger.LogWarning("No user found for email verification token: {Token}", token);
                    return false;
                }

                // is less than 15 minutes old
                if (user.LastEmailVerificationSentAt.HasValue &&
                    (DateTime.UtcNow - user.LastEmailVerificationSentAt.Value).TotalMinutes > 15)
                {
                    _logger.LogWarning("Email verification token for user {UserId} is expired", user.Id);
                    return false;
                }

                // Mark the email as verified
                user.EmailConfirmed = true;
                user.EmailConfirmedAt = DateTime.UtcNow;

                // Update the user
                await _userService.UpdateAsync(user);
                return true;

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
