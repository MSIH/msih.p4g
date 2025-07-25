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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSIH.Core.Common.Data;
using MSIH.Core.Services.Messages.Models;

namespace MSIH.Core.Services.Messages.Data
{
    /// <summary>
    /// Data seeder for MessageTemplate data
    /// </summary>
    public class MessageTemplateDataSeeder
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MessageTemplateDataSeeder> _logger;

        public MessageTemplateDataSeeder(
            IServiceProvider serviceProvider,
            ILogger<MessageTemplateDataSeeder> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Seeds initial message template data if none exists
        /// </summary>
        public async Task SeedAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                _logger.LogInformation("Checking for missing message templates...");

                // Define all templates that should exist
                var templateDefinitions = GetTemplateDefinitions();

                // Get existing templates to check which ones are missing
                var existingTemplates = await dbContext.Set<MessageTemplate>()
                    .Select(t => t.Name)
                    .ToListAsync();

                var templatesToAdd = new List<MessageTemplate>();

                // Check each template definition and add if missing
                foreach (var templateDef in templateDefinitions)
                {
                    if (!existingTemplates.Contains(templateDef.Name, StringComparer.OrdinalIgnoreCase))
                    {
                        templatesToAdd.Add(templateDef);
                        _logger.LogInformation("Found missing template: {TemplateName}", templateDef.Name);
                    }
                }

                // Add missing templates to the database
                if (templatesToAdd.Any())
                {
                    _logger.LogInformation("Adding {Count} missing message templates...", templatesToAdd.Count);

                    foreach (var template in templatesToAdd)
                    {
                        dbContext.Set<MessageTemplate>().Add(template);
                    }

                    await dbContext.SaveChangesAsync();
                    _logger.LogInformation("Successfully added {Count} message templates.", templatesToAdd.Count);
                }
                else
                {
                    _logger.LogInformation("All message templates already exist. No templates added.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding message template data.");
                throw;
            }
        }

        /// <summary>
        /// Returns all the template definitions that should exist in the system
        /// </summary>
        private MessageTemplate[] GetTemplateDefinitions()
        {
            return new[]
            {
                // Email Verification Template
                new MessageTemplate
                {
                    Name = "Email Verification",
                    Description = "Email sent to users to verify their email address",
                    MessageType = "Email",
                    Category = "EmailVerification",
                    DefaultSubject = "Please Verify Your Email Address",
                    DefaultSender = "donations@msih.org",
                    TemplateContent = """
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
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
    <p><a href="{{verificationUrl}}">{{verificationUrl}}</a></p>
    <p>If you did not request this email, please ignore it.</p>
</body>
</html>
""",
                    IsHtml = true,
                    AvailablePlaceholders = "userName, verificationUrl",
                    IsDefault = true,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = "MessageService"
                },

                // Donor Thank You Email Template
                new MessageTemplate
                {
                    Name = "Donor Thank You Email",
                    Description = "Email sent to donors after they make a donation",
                    MessageType = "Email",
                    Category = "ThankYou",
                    DefaultSubject = "Thank You for Your Donation to {{organizationName}}",
                    DefaultSender = "donations@msih.org",
                    TemplateContent = """
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Thank You for Your Donation</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }
        h1 { color: #2c5282; }
        .signature { margin-top: 30px; }
        .tax-info { margin-top: 20px; font-style: italic; }
        .description { margin: 20px 0; }
    </style>
</head>
<body>
    <h1>Thank You for Your Donation</h1>

    <p>Dear {{donorName}},</p>

    <p>Thank you for making a tax-deductible donation today in the amount of {{donationAmountInDollars}} to {{organizationName}}  ({{organizationURL}}), a tax-exempt organization under Section 501(c)(3) of the Internal Revenue Code, EIN {{ein}}. No goods or services were provided in exchange for this donation.</p>

    <div class="tax-info">
        <p>Please retain this receipt for your tax records.</p>
    </div>

    <div class="description">
        <p>{{organizationShortDescription}}</p>
        <p>{{projectShortDescription}}</p>
    </div>

    <div class="signature">
        <p>Sincerely,</p>
        <p>{{fundraiserName}}<br>{{fundraiserTitle}}</p>
    </div>

    <div class="social-sharing" style="margin-top: 25px; padding: 15px; background-color: #f0f4f8; border-radius: 8px;">
        <p><strong>Support our mission - share with your network!</strong></p>
        <p>Your donation makes a difference. Help us reach more people by sharing our donation page:</p>
        <p><a href="{{donationUrl}}">{{donationUrl}}</a></p>

        <div style="margin-top: 15px;">
            <p><strong>Sample messages to share:</strong></p>
            <div style="background-color: #e2e8f0; padding: 10px; margin-bottom: 10px; border-radius: 4px;">
                <p><strong>Email:</strong> I just donated to {{organizationName}} to support {{projectShortDescription}}. Join me in making a difference: {{donationUrl}}</p>
            </div>
            <div style="background-color: #e2e8f0; padding: 10px; margin-bottom: 10px; border-radius: 4px;">
                <p><strong>Facebook:</strong> Proud to support {{organizationName}} and their mission to {{organizationMission}}. Every donation counts! {{donationUrl}}</p>
            </div>
            <div style="background-color: #e2e8f0; padding: 10px; border-radius: 4px;">
                <p><strong>Twitter:</strong> Just supported {{organizationName}}! They're doing amazing work with {{projectShortDescription}}. Join me! {{donationUrl}} #MakeADifference</p>
            </div>
        </div>
    </div>

</body>
</html>
""",
                    IsHtml = true,
                    AvailablePlaceholders = "donorName, donationAmountInDollars, organizationName, ein, organizationShortDescription, projectShortDescription, fundraiserName, fundraiserTitle, donationUrl, organizationMission",
                    IsDefault = false, // Not default since MVP version exists
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = "MessageService"
                },

                // MVP Donor Thank You Email Template
                new MessageTemplate
                {
                    Name = "MVP Donor Thank You Email",
                    Description = "Email sent to donors after they make a donation",
                    MessageType = "Email",
                    Category = "ThankYou",
                    DefaultSubject = "Thank You for Your Donation to Make Sure It Happens Inc",
                    DefaultSender = "donations@msih.org",
                    TemplateContent = """
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Thank You for Your Donation</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }
        h1 { color: #2c5282; }
        .signature { margin-top: 30px; }
        .tax-info { margin-top: 20px; font-style: italic; }
        .description { margin: 20px 0; }
    </style>
</head>
<body>
    <h1>Thank You for Your Donation</h1>

    <p>Dear {{donorName}},</p>

    <p>Thank you for making a tax-deductible donation today in the amount of {{donationAmountInDollars}} to Make Sure It Happens Inc (https://www.msih.org), a tax-exempt organization under Section 501(c)(3) of the Internal Revenue Code, EIN 5-3536160. No goods or services were provided in exchange for this donation.</p>

    <div class="tax-info">
        <p>Please retain this receipt for your tax records.</p>
    </div>


        <p>To view your donation history, please log in <a href="https://msih.org/login">https://msih.org/login</a></p>


    <div class="social-sharing" style="margin-top: 25px; padding: 15px; background-color: #f0f4f8; border-radius: 8px;">
        <p>Your donation makes a difference. Help us reach more people by sharing:<a href="https://msih.org/donate">https://msih.org/donate</a></p>

        <div style="margin-top: 15px;">
            <p><strong>Sample messages to share:</strong></p>
            <div style="background-color: #e2e8f0; padding: 10px; margin-bottom: 10px; border-radius: 4px;">
                <p><strong>Email friends and family:</strong> I just donated to Make Sure It Happens Inc. They're doing amazing work. Join me in making a difference: https://msih.org/donate</p>
            </div>
            <div style="background-color: #e2e8f0; padding: 10px; margin-bottom: 10px; border-radius: 4px;">
                <p><strong>Post to Facebook:</strong> Proud to support Make Sure It Happens Inc. They're doing amazing work. Every donation counts! https://msih.org/donate</p>
            </div>
        </div>
    </div>

    <div class="signature">
        <p>Sincerely,</p>
        <p>Barry Schenider<br>President and Chairman of the Board</p>
    </div>

    <div class="org-info">
        <p>Make Sure It Happens Inc. | 5013 Russsett Road, Rockville, Maryland | EIN: EIN 5-3536160 | www.msih.org</p>
    </div>

</body>
</html>
""",
                    IsHtml = true,
                    AvailablePlaceholders = "donorName, donationAmountInDollars, organizationName, ein, organizationShortDescription, projectShortDescription, fundraiserName, fundraiserTitle, donationUrl, organizationMission",
                    IsDefault = true,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = "MessageService"
                },

                // Fundraiser Report Email Template
                new MessageTemplate
                {
                    Name = "Fundraiser Status Report",
                    Description = "Weekly or monthly email providing status update to fundraisers",
                    MessageType = "Email",
                    Category = "FundraiserReport",
                    DefaultSubject = "Your Fundraising Campaign Status Report",
                    DefaultSender = "reports@msih.org",
                    TemplateContent = """
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Fundraising Campaign Report</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }
        h1 { color: #2c5282; }
        h2 { color: #4a5568; margin-top: 30px; }
        .highlight { font-weight: bold; color: #2b6cb0; }
        table { width: 100%; border-collapse: collapse; margin: 20px 0; }
        th, td { padding: 12px; text-align: left; border-bottom: 1px solid #e2e8f0; }
        th { background-color: #edf2f7; }
    </style>
</head>
<body>
    <h1>Fundraising Campaign Status Report</h1>

    <p>Dear {{fundraiserName}},</p>

    <p>Here's your {{reportPeriod}} fundraising report for the period ending {{reportEndDate}}.</p>

    <h2>Campaign Summary</h2>
    <p>Campaign: <strong>{{campaignName}}</strong></p>
    <p>Total Donations: <span class="highlight">{{totalDonationCount}}</span></p>
    <p>Total Amount Raised: <span class="highlight">{{totalAmountRaised}}</span></p>
    <p>Average Donation: {{averageDonationAmount}}</p>

    <p>Thank you for your continued efforts in raising funds for this important cause!</p>

    <p>Share your unique referral link to get more supporters: <a href="{{referralUrl}}">{{referralUrl}}</a></p>

    <p>You can view all your campaign details and donations from <a href="{{fundraiserUrl}}">{{fundraiserUrl}}</a></p>

    <p>Best regards,<br>The {{organizationName}} Team</p>

    <h2>Recent Donations</h2>
    <table>
        <thead>
            <tr>
                <th>Date</th>
                <th>Donor</th>
                <th>Amount</th>
            </tr>
        </thead>
        <tbody>
            {{recentDonationsTable}}
        </tbody>
    </table>

</body>
</html>
""",
                    IsHtml = true,
                    AvailablePlaceholders = "fundraiserName, reportPeriod, reportEndDate, campaignName, totalDonationCount, totalAmountRaised, averageDonationAmount, recentDonationsTable, organizationName, referralUrl, fundraiserUrl",
                    IsDefault = true,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = "MessageService"
                },

                // End of Year Tax Letter to Donor
                new MessageTemplate
                {
                    Name = "End of Year Tax Receipt",
                    Description = "Annual tax receipt sent to donors at the end of the year",
                    MessageType = "Email",
                    Category = "TaxReceipt",
                    DefaultSubject = "Your {{year}} Tax Receipt from {{organizationName}}",
                    DefaultSender = "tax@msih.org",
                    TemplateContent = """
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Annual Tax Receipt</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }
        h1 { color: #2c5282; }
        h2 { color: #4a5568; margin-top: 30px; }
        .highlight { font-weight: bold; color: #2b6cb0; }
        table { width: 100%; border-collapse: collapse; margin: 20px 0; }
        th, td { padding: 12px; text-align: left; border-bottom: 1px solid #e2e8f0; }
        th { background-color: #edf2f7; }
        .tax-info { margin-top: 30px; font-style: italic; }
        .header-info { margin-bottom: 30px; }
        .org-info { margin-top: 40px; font-size: 0.9em; }
    </style>
</head>
<body>
    <div class="header-info">
        <p>{{currentDate}}</p>
        <p>{{donorName}}<br>
        {{donorAddress}}</p>
    </div>

    <h1>{{year}} Tax Receipt</h1>

    <p>Dear {{donorFirstName}},</p>

    <p>Thank you for your generous support of {{organizationName}} during {{year}}. Your contributions have made a significant impact on our mission to {{organizationMission}}.</p>

    <p>This letter serves as your official tax receipt for the calendar year {{year}}. {{organizationName}} is a tax-exempt organization under Section 501(c)(3) of the Internal Revenue Code, EIN {{ein}}. No goods or services were provided in exchange for these donations.</p>

    <h2>Summary of Your {{year}} Donations</h2>

    <table>
        <thead>
            <tr>
                <th>Date</th>
                <th>Campaign</th>
                <th>Amount</th>
            </tr>
        </thead>
        <tbody>
            {{donationsList}}
        </tbody>
        <tfoot>
            <tr>
                <td colspan="2"><strong>Total Donations for {{year}}</strong></td>
                <td><strong>{{totalYearlyDonations}}</strong></td>
            </tr>
        </tfoot>
    </table>

    <div class="tax-info">
        <p>Please retain this receipt for your tax records. If you have any questions regarding this information, please contact us at {{organizationEmail}} or {{organizationPhone}}.</p>
    </div>

    <p>Thank you again for your generous support.</p>

     <div class="social-sharing" style="margin-top: 25px; padding: 15px; background-color: #f0f4f8; border-radius: 8px;">
        <p><strong>Support our mission - share with your network!</strong></p>
        <p>Your donation makes a difference. Help us reach more people by sharing our donation page:</p>
        <p><a href="{{donationUrl}}">{{donationUrl}}</a></p>

        <div style="margin-top: 15px;">
            <p><strong>Sample messages to share:</strong></p>
            <div style="background-color: #e2e8f0; padding: 10px; margin-bottom: 10px; border-radius: 4px;">
                <p><strong>Email:</strong> I just donated to {{organizationName}} to support {{projectShortDescription}}. Join me in making a difference: {{donationUrl}}</p>
            </div>
            <div style="background-color: #e2e8f0; padding: 10px; margin-bottom: 10px; border-radius: 4px;">
                <p><strong>Facebook:</strong> Proud to support {{organizationName}} and their mission to {{organizationMission}}. Every donation counts! {{donationUrl}}</p>
            </div>
            <div style="background-color: #e2e8f0; padding: 10px; border-radius: 4px;">
                <p><strong>Twitter:</strong> Just supported {{organizationName}}! They're doing amazing work with {{projectShortDescription}}. Join me! {{donationUrl}} #MakeADifference</p>
            </div>
        </div>
    </div>

    <p>Sincerely,</p>
    <p>{{organizationLeaderName}}<br>{{organizationLeaderTitle}}<br>{{organizationName}}</p>

    <div class="org-info">
        <p>{{organizationName}}<br>
        {{organizationAddress}}<br>
        EIN: {{ein}}</p>
    </div>
</body>
</html>
""",
                    IsHtml = true,
                    AvailablePlaceholders = "currentDate, donorName, donorFirstName, donorAddress, year, organizationName, organizationMission, ein, donationsList, totalYearlyDonations, organizationEmail, organizationPhone, organizationLeaderName, organizationLeaderTitle, organizationAddress, donationUrl, projectShortDescription",
                    IsDefault = true,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = "MessageService"
                },

                // Fundraiser Notice of Donation
                new MessageTemplate
                {
                    Name = "Fundraiser Donation Notification",
                    Description = "Notification to fundraisers when a donation is made to their campaign",
                    MessageType = "Email",
                    Category = "DonationNotification",
                    DefaultSubject = "New Donation to Your Fundraising Campaign",
                    DefaultSender = "notifications@msih.org",
                    TemplateContent = """
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>New Donation Notification</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }
        h1 { color: #2c5282; }
        .highlight { font-weight: bold; color: #2b6cb0; }
        .donation-details { background-color: #f7fafc; padding: 20px; border-radius: 8px; margin: 20px 0; }
        .donor-message { margin-top: 20px; font-style: italic; background-color: #f0fff4; padding: 15px; border-left: 4px solid #68d391; }
    </style>
</head>
<body>
    <h1>You've Received a New Donation!</h1>

    <p>Dear {{fundraiserName}},</p>

    <p>Great news! Someone has just made a donation to your fundraising campaign.</p>

    <div class="donation-details">
        <p><strong>Campaign:</strong> {{campaignName}}</p>
        <p><strong>Donor:</strong> {{donorFirstName}}</p>
        <p><strong>Amount:</strong> <span class="highlight">{{donationAmount}}</span></p>
        <p><strong>Date:</strong> {{donationDate}}</p>
    </div>

    <p>This brings your total raised to {{totalRaised}}. Keep up the great work!</p>

    <p>You can view all your campaign details and donations from <a href="{{fundraiserUrl}}">{{fundraiserUrl}}</a></p>

    <p>Share our donation page to get more supporters: <a href="{{donationUrl}}">{{donationUrl}}</a></p>

    <p>Thank you for your continued efforts in raising funds for this important cause!</p>

    <div class="social-sharing" style="margin-top: 20px; padding: 15px; background-color: #f0f4f8; border-radius: 8px;">
        <p><strong>Sample messages to share with your network:</strong></p>
        <div style="background-color: #e2e8f0; padding: 10px; margin-bottom: 10px; border-radius: 4px;">
            <p><strong>Email:</strong> I'm fundraising for {{organizationName}} and just received a new donation! Join in supporting this important cause: {{donationUrl}}</p>
        </div>
        <div style="background-color: #e2e8f0; padding: 10px; margin-bottom: 10px; border-radius: 4px;">
            <p><strong>Facebook:</strong> Excited to share that my fundraising campaign for {{organizationName}} just received another donation! We're at {{totalRaised}} and counting. Support our campaign: {{donationUrl}}</p>
        </div>
        <div style="background-color: #e2e8f0; padding: 10px; border-radius: 4px;">
            <p><strong>Twitter:</strong> Every donation counts! My {{campaignName}} campaign just hit {{totalRaised}}. Help us reach our goal: {{donationUrl}} #Fundraising</p>
        </div>
    </div>

    <p>Best regards,<br>The {{organizationName}} Team</p>
</body>
</html>
""",
                    IsHtml = true,
                    AvailablePlaceholders = "fundraiserName, campaignName, donorFirstName, donationAmount, donationDate, donorMessage, totalRaised, organizationName, donationUrl, fundraiserUrl",
                    IsDefault = true,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = "MessageService"
                },

                // Donor Thank You SMS
                new MessageTemplate
                {
                    Name = "Donor Thank You SMS",
                    Description = "SMS sent to donors after they make a donation",
                    MessageType = "SMS",
                    Category = "ThankYou",
                    TemplateContent = "Thank you {{donorFirstName}} for your generous donation of {{donationAmountInDollars}} to {{organizationName}}! Your contribution helps us {{organizationMission}}. A tax receipt has been emailed to you. Support our cause by sharing our donation page: {{donationUrl}}",
                    IsHtml = false,
                    AvailablePlaceholders = "donorFirstName, donationAmountInDollars, organizationName, organizationMission, donationUrl",
                    IsDefault = true,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = "MessageService"
                },

                // Fundraiser Donation Notification SMS
                new MessageTemplate
                {
                    Name = "Fundraiser Donation Notification SMS",
                    Description = "SMS notification to fundraisers when a donation is made",
                    MessageType = "SMS",
                    Category = "DonationNotification",
                    TemplateContent = "Great news {{fundraiserName}}! {{donorFirstName}} just donated {{donationAmount}} to your {{campaignName}} campaign. Your total raised is now {{totalRaised}}! Share our donation page to get more supporters: {{donationUrl}}",
                    IsHtml = false,
                    AvailablePlaceholders = "fundraiserName, donorFirstName, donationAmount, campaignName, totalRaised, donationUrl",
                    IsDefault = true,
                    IsActive = true,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = "MessageService"
                }
            };
        }
    }
}
