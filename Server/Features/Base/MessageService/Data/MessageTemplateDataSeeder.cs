/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Features.Base.MessageService.Models;

namespace msih.p4g.Server.Features.Base.MessageService.Data
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

                // Check if we need to seed templates
                if (!await dbContext.Set<MessageTemplate>().AnyAsync(t => 
                    t.Category == "ThankYou" || 
                    t.Category == "FundraiserReport" || 
                    t.Category == "TaxReceipt" || 
                    t.Category == "DonationNotification"))
                {
                    _logger.LogInformation("Seeding message template data...");

                    // 1. Donor Thank You Email Template
                    var donorThanksTemplate = new MessageTemplate
                    {
                        Name = "Donor Thank You Email",
                        Description = "Email sent to donors after they make a donation",
                        MessageType = "Email",
                        Category = "ThankYou",
                        DefaultSubject = "Thank You for Your Donation to {{organizationName}}",
                        DefaultSender = "donations@msih.org",
                        TemplateContent = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
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
    
    <p>Thank you for making a tax-deductible donation today in the amount of {{donationAmountInDollars}} to {{organizationName}}, a tax-exempt organization under Section 501(c)(3) of the Internal Revenue Code, EIN {{ein}}. No goods or services were provided in exchange for this donation.</p>
    
    <div class=""description"">
        <p>{{organizationShortDescription}}</p>
        <p>{{projectShortDescription}}</p>
    </div>
    
    <div class=""signature"">
        <p>Sincerely,</p>
        <p>{{fundraiserName}}<br>{{fundraiserTitle}}</p>
    </div>
    
    <div class=""tax-info"">
        <p>Please retain this receipt for your tax records.</p>
    </div>
</body>
</html>",
                        IsHtml = true,
                        AvailablePlaceholders = "donorName, donationAmountInDollars, organizationName, ein, organizationShortDescription, projectShortDescription, fundraiserName, fundraiserTitle",
                        IsDefault = true,
                        IsActive = true,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = "System"
                    };

                    // 2. Fundraiser Report Email Template
                    var fundraiserReportTemplate = new MessageTemplate
                    {
                        Name = "Fundraiser Status Report",
                        Description = "Weekly or monthly email providing status update to fundraisers",
                        MessageType = "Email",
                        Category = "FundraiserReport",
                        DefaultSubject = "Your Fundraising Campaign Status Report",
                        DefaultSender = "reports@msih.org",
                        TemplateContent = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
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
    <p>Total Donations: <span class=""highlight"">{{totalDonationCount}}</span></p>
    <p>Total Amount Raised: <span class=""highlight"">{{totalAmountRaised}}</span></p>
    <p>Average Donation: {{averageDonationAmount}}</p>
    
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
    
    <p>Thank you for your continued efforts in raising funds for this important cause!</p>
    
    <p>Share your unique referral link to get more supporters: <a href=""{{referralUrl}}"">{{referralUrl}}</a></p>
    
    <p>Best regards,<br>The {{organizationName}} Team</p>
</body>
</html>",
                        IsHtml = true,
                        AvailablePlaceholders = "fundraiserName, reportPeriod, reportEndDate, campaignName, totalDonationCount, totalAmountRaised, averageDonationAmount, recentDonationsTable, organizationName,  referralUrl",
                        IsDefault = true,
                        IsActive = true,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = "System"
                    };

                    // 3. End of Year Tax Letter to Donor
                    var taxLetterTemplate = new MessageTemplate
                    {
                        Name = "End of Year Tax Receipt",
                        Description = "Annual tax receipt sent to donors at the end of the year",
                        MessageType = "Email",
                        Category = "TaxReceipt",
                        DefaultSubject = "Your {{year}} Tax Receipt from {{organizationName}}",
                        DefaultSender = "tax@msih.org",
                        TemplateContent = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
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
    <div class=""header-info"">
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
                <td colspan=""2""><strong>Total Donations for {{year}}</strong></td>
                <td><strong>{{totalYearlyDonations}}</strong></td>
            </tr>
        </tfoot>
    </table>
    
    <div class=""tax-info"">
        <p>Please retain this receipt for your tax records. If you have any questions regarding this information, please contact us at {{organizationEmail}} or {{organizationPhone}}.</p>
    </div>
    
    <p>Thank you again for your generous support.</p>
    
    <p>Sincerely,</p>
    <p>{{organizationLeaderName}}<br>{{organizationLeaderTitle}}<br>{{organizationName}}</p>
    
    <div class=""org-info"">
        <p>{{organizationName}}<br>
        {{organizationAddress}}<br>
        EIN: {{ein}}</p>
    </div>
</body>
</html>",
                        IsHtml = true,
                        AvailablePlaceholders = "currentDate, donorName, donorFirstName, donorAddress, year, organizationName, organizationMission, ein, donationsList, totalYearlyDonations, organizationEmail, organizationPhone, organizationLeaderName, organizationLeaderTitle, organizationAddress",
                        IsDefault = true,
                        IsActive = true,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = "System"
                    };

                    // 4. Fundraiser Notice of Donation
                    var donationNoticeTemplate = new MessageTemplate
                    {
                        Name = "Fundraiser Donation Notification",
                        Description = "Notification to fundraisers when a donation is made to their campaign",
                        MessageType = "Email",
                        Category = "DonationNotification",
                        DefaultSubject = "New Donation to Your Fundraising Campaign",
                        DefaultSender = "notifications@msih.org",
                        TemplateContent = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
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
    
    <div class=""donation-details"">
        <p><strong>Campaign:</strong> {{campaignName}}</p>
        <p><strong>Donor:</strong> {{donorFirstName}}</p>
        <p><strong>Amount:</strong> <span class=""highlight"">{{donationAmount}}</span></p>
        <p><strong>Date:</strong> {{donationDate}}</p>
    </div>
    
    @if(!string.IsNullOrEmpty(donorMessage))
    {
        <div class=""donor-message"">
            <p><strong>Message from the donor:</strong></p>
            <p>""{{donorMessage}}""</p>
        </div>
    }
    
    <p>This brings your total raised to {{totalRaised}}. Keep up the great work!</p>
    
    <p>You can view all your campaign details and donations by logging into your account.</p>
    
    <p>Share your unique referral link to get more supporters: <a href=""{{referralUrl}}"">{{referralUrl}}</a></p>   
    
    <p>Thank you for your continued efforts in raising funds for this important cause!</p>
    
    <p>Best regards,<br>The {{organizationName}} Team</p>
</body>
</html>",
                        IsHtml = true,
                        AvailablePlaceholders = "fundraiserName, campaignName, donorFirstName, donationAmount, donationDate, donorMessage, totalRaised, organizationName,  referralUrl",
                        IsDefault = true,
                        IsActive = true,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = "System"
                    };

                    // Add SMS versions of templates
                    var donorThanksSmsTemplate = new MessageTemplate
                    {
                        Name = "Donor Thank You SMS",
                        Description = "SMS sent to donors after they make a donation",
                        MessageType = "SMS",
                        Category = "ThankYou",
                        TemplateContent = "Thank you {{donorFirstName}} for your generous donation of {{donationAmountInDollars}} to {{organizationName}}! Your contribution helps us {{organizationMission}}. A tax receipt has been emailed to you.",
                        IsHtml = false,
                        AvailablePlaceholders = "donorFirstName, donationAmountInDollars, organizationName, organizationMission",
                        IsDefault = true,
                        IsActive = true,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = "System"
                    };

                    var fundraiserNotificationSmsTemplate = new MessageTemplate
                    {
                        Name = "Fundraiser Donation Notification SMS",
                        Description = "SMS notification to fundraisers when a donation is made",
                        MessageType = "SMS",
                        Category = "DonationNotification",
                        TemplateContent = "Great news {{fundraiserName}}! {{donorFirstName}} just donated {{donationAmount}} to your {{campaignName}} campaign. Your total raised is now {{totalRaised}}! Share your campaign with referral link {{referralUrl}}",
                        IsHtml = false,
                        AvailablePlaceholders = "fundraiserName, donorFirstName, donationAmount, campaignName, totalRaised, referralUrl",
                        IsDefault = true,
                        IsActive = true,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = "System"
                    };

                    // Add all templates to the database
                    dbContext.Set<MessageTemplate>().Add(donorThanksTemplate);
                    dbContext.Set<MessageTemplate>().Add(fundraiserReportTemplate);
                    dbContext.Set<MessageTemplate>().Add(taxLetterTemplate);
                    dbContext.Set<MessageTemplate>().Add(donationNoticeTemplate);
                    dbContext.Set<MessageTemplate>().Add(donorThanksSmsTemplate);
                    dbContext.Set<MessageTemplate>().Add(fundraiserNotificationSmsTemplate);

                    await dbContext.SaveChangesAsync();

                    _logger.LogInformation("Message template data seeded successfully.");
                }
                else
                {
                    _logger.LogInformation("Message template data already exists. Skipping seed.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding message template data.");
                throw;
            }
        }
    }
}
