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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MSIH.Core.Common.Utilities;
using MSIH.Core.Services.Message.Interfaces;
using MSIH.Core.Services.Setting.Interfaces;
using MSIH.Core.Services.User.Interfaces;
using UserEntity = MSIH.Core.Services.User.Models.User;

namespace MSIH.Core.Services.User.Services
{
    public class EmailVerificationService : IEmailVerificationService
    {
        private readonly IUserService _userService;
        private readonly IMessageService _messageService;
        private readonly ISettingsService _settingsService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailVerificationService> _logger;
        private readonly ReferralLinkHelper _referralLinkHelper;

        public EmailVerificationService(
            IUserService userService,
            IMessageService messageService,
            ISettingsService settingsService,
            IConfiguration configuration,
            ILogger<EmailVerificationService> logger,
            ReferralLinkHelper referralLinkHelper)
        {
            _userService = userService;
            _messageService = messageService;
            _settingsService = settingsService;
            _configuration = configuration;
            _logger = logger;
            _referralLinkHelper = referralLinkHelper;
        }

        public async Task<bool> SendVerificationEmailAsync(UserEntity user)
        {
            try
            {
                if (user == null)
                    throw new ArgumentNullException(nameof(user));

                // Get the base URL for the verification link
                var baseUrl = await _settingsService.GetValueAsync("BaseUrl")
                    ?? _configuration["BaseUrl"]
                    ?? "https://msih.org";
                baseUrl = baseUrl.TrimEnd('/');

                var donationUrl = await _settingsService.GetValueAsync("donationUrl")
                    ?? _configuration["donationUrl"]
                    ?? $"{baseUrl}/donate";
                donationUrl = donationUrl.TrimEnd('/');

                // based on the current time get number in format of HHmmss
                var currentTime = DateTime.UtcNow.ToString("HHmmss");
                // covert to int
                var currentTimeInt = int.Parse(currentTime);

                // Generate a verification token
                var token = RandomStringGenerator.Generate(user.Id + currentTimeInt, 8, RandomStringGenerator.CharSet.All);

                // Create the verification link
                var verificationLink = $"{baseUrl}/verify-email?token={token}";

                var referalURL = await _referralLinkHelper.GenerateReferralLinkAsync(user.Profile, donationUrl, appendName: true); // Updated to define referalURL


                // Set up email placeholders
                var placeholders = new Dictionary<string, string>
                {
                    { "fullName", user.Profile.FullName },
                    { "VerificationLink", verificationLink },
                    { "token", token },
                    { "referalURL", referalURL} // Updated to use the new donationUrl
                };
                var TemplateContent = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <title>Email Verification</title>
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #ffffff;"">
    <h1 style=""color: #2c5282; margin-bottom: 20px; font-size: 16px;"">Email Verification Required</h1>
    
    <p style=""margin-bottom: 16px;"">Dear {{fullName}},</p>
    
    <p style=""margin-bottom: 16px;"">Please click the link below to verify your email address:</p>
    
    <p style=""margin-bottom: 20px;"">
        <a href=""{{verificationUrl}}"" style=""color: #3182ce; text-decoration: underline;"">{{verificationUrl}}</a>
    </p>
    
    <p style=""margin-bottom: 12px;"">Or copy and paste this code into the app:</p>
    
    <div style=""background: #f4f4f4; border: 1px solid #e2e8f0; border-radius: 4px; padding: 12px; font-family: 'Consolas', 'Courier New', monospace; font-size: 1.1em; margin: 16px 0; word-break: break-all; display: inline-block; min-width: 200px; text-align: center;"">
        {{token}}
    </div>
    
    <p style=""margin-top: 20px; color: #666; font-size: 0.95em;"">If you did not request this email, please ignore it.</p>
    
    <p style=""margin-top: 20px; color: #666; font-size: 0.95em;"">Use this donation referral URL for any sharing: {{referalURL}}</p>
    
    <div style=""margin-top: 30px; padding-top: 20px; border-top: 1px solid #e2e8f0;"">
        <p style=""margin: 0; color: #555; font-size: 0.9em;"">
            Best regards,<br>
            <strong>Make Sure It Happens</strong><br>
            <a href=""https://www.msih.org"" style=""color: #3182ce; text-decoration: none;"">https://www.msih.org</a>
        </p>
    </div>
</body>
</html>";

                // Replace placeholders in the template content
                TemplateContent = TemplateContent
                    .Replace("{{fullName}}", placeholders["fullName"])
                    .Replace("{{verificationUrl}}", placeholders["VerificationLink"])
                    .Replace("{{token}}", placeholders["token"])
                    .Replace("{{referalURL}}", placeholders["referalURL"]);

                var emailSent = await _messageService.SendEmailAsync(user.Email, "Verify Email Address", TemplateContent);
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

                // is less than 90 days old (129,600 minutes)
                if (user.LastEmailVerificationSentAt.HasValue &&
                    (DateTime.UtcNow - user.LastEmailVerificationSentAt.Value).TotalMinutes > 129600) //TODO: make this use settings and creat environment variable
                {
                    _logger.LogWarning("Email verification token for user {UserId} is expired (older than 90 days)", user.Id);
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
