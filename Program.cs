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

using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using msih.p4g.Client.Features.Authentication.Services;
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Common.Data.Extensions;
using msih.p4g.Server.Common.Interfaces;
using msih.p4g.Server.Common.Services;
using msih.p4g.Server.Common.Utilities;
using msih.p4g.Server.Features.Base.AffiliateMonitoringService.Extensions;
using msih.p4g.Server.Features.Base.EmailService.Extensions;
using msih.p4g.Server.Features.Base.MessageService.Data;
using msih.p4g.Server.Features.Base.MessageService.Extensions;
using msih.p4g.Server.Features.Base.PaymentService.Extensions;
using msih.p4g.Server.Features.Base.PaymentService.Interfaces;
using msih.p4g.Server.Features.Base.PayoutService.Extensions;
using msih.p4g.Server.Features.Base.ProfileService.Interfaces;
using msih.p4g.Server.Features.Base.ProfileService.Repositories;
using msih.p4g.Server.Features.Base.ProfileService.Services;
using msih.p4g.Server.Features.Base.SettingsService.Extensions;
using msih.p4g.Server.Features.Base.SettingsService.Services;
using msih.p4g.Server.Features.Base.SmsService.Extensions;
using msih.p4g.Server.Features.Base.UserProfileService.Interfaces;
using msih.p4g.Server.Features.Base.UserProfileService.Services;
using msih.p4g.Server.Features.Base.UserService.Interfaces;
using msih.p4g.Server.Features.Base.UserService.Repositories;
using msih.p4g.Server.Features.Base.UserService.Services;
using msih.p4g.Server.Features.Base.W9FormService.Interfaces;
using msih.p4g.Server.Features.Base.W9FormService.Services;
using msih.p4g.Server.Features.CampaignService.Data;
using msih.p4g.Server.Features.CampaignService.Interfaces;
using msih.p4g.Server.Features.CampaignService.Repositories;
using msih.p4g.Server.Features.CampaignService.Services;
using msih.p4g.Server.Features.DonationService.Extensions;
using msih.p4g.Server.Features.DonationService.Interfaces;
using msih.p4g.Server.Features.DonationService.Repositories;
using msih.p4g.Server.Features.DonationService.Services;
using msih.p4g.Server.Features.DonorService.Interfaces;
using msih.p4g.Server.Features.DonorService.Repositories;
using msih.p4g.Server.Features.DonorService.Services;
using msih.p4g.Server.Features.FundraiserService.Interfaces;
using msih.p4g.Server.Features.FundraiserService.Repositories;
using msih.p4g.Server.Features.FundraiserService.Services;
using msih.p4g.Server.Features.OrganizationService.Data;
using msih.p4g.Server.Features.OrganizationService.Interfaces;
using msih.p4g.Server.Features.OrganizationService.Repositories;
using msih.p4g.Server.Features.OrganizationService.Services;

var builder = WebApplication.CreateBuilder(args);

//Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.RootDirectory = "/Server/Pages";
});
//builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers(); // Add this line to enable API controllers

// Register the AuthService as a scoped service so it's created per user session
builder.Services.AddScoped<AuthService>();

// Add HttpContextAccessor for accessing current user information
builder.Services.AddHttpContextAccessor();

// Add Data Protection services for sensitive data encryption
builder.Services.AddDataProtection()
    .SetApplicationName("msih.p4g")
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "keys")));

// Add Entity Framework with the unified ApplicationDbContext
DatabaseConfigurationHelper.AddConfiguredDbContext<ApplicationDbContext>(
    builder.Services,
    builder.Configuration,
    builder.Environment);

// Also register ApplicationDbContext directly for migration and seeding operations
builder.Services.AddScoped<ApplicationDbContext>(serviceProvider =>
{
    var contextFactory = serviceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    return contextFactory.CreateDbContext();
});

// Register Email Service using the extension method
builder.Services.AddEmailServices(builder.Configuration);

// Register SMS Service and related dependencies
builder.Services.AddSmsServices(builder.Configuration, builder.Environment);

// Register Message Service (for both email and SMS) and related dependencies
builder.Services.AddMessageServices(builder.Configuration, builder.Environment);

// Register Payment Service and related dependencies
builder.Services.AddPaymentServices(builder.Configuration, builder.Environment);

// Register Payout Service and related dependencies
builder.Services.AddPayoutServices(builder.Configuration, builder.Environment);

// Register IPaymentService using a factory (resolves dependency injection error)
builder.Services.AddScoped<IPaymentService>(provider =>
{
    var factory = provider.GetRequiredService<IPaymentServiceFactory>();
    return factory.GetDefaultPaymentService();
});

// Register Settings Service and related dependencies
builder.Services.AddSettingsServices();

// Register Affiliate Monitoring Service
builder.Services.AddAffiliateMonitoringServices();

// Register common utilities
builder.Services.AddScoped<ReferralURLGenerator>();

// Register DonorRepository and DonorService for DI
builder.Services.AddScoped<IDonorRepository, DonorRepository>();
builder.Services.AddScoped<IDonorService, DonorService>();

// Register CampaignRepository and CampaignService for DI
builder.Services.AddSingleton<ICampaignRepository, CampaignRepository>();
builder.Services.AddScoped<ICampaignService, CampaignService>();

// Register ProfileRepository and ProfileService for DI
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IProfileService, ProfileService>();

// Register DonationRepository and DonationService for DI
builder.Services.AddScoped<IDonationRepository, DonationRepository>();
builder.Services.AddScoped<IDonationService, DonationService>();

// Register recurring donation background processing
builder.Services.AddRecurringDonationProcessing();

// Register UserRepository and UserService for DI
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<msih.p4g.Client.Common.Services.AuthorizationService>();

// Register UserProfileService for coordinating User and Profile operations
builder.Services.AddScoped<IUserProfileService, UserProfileService>();

// Register FundraiserRepository and FundraiserService for DI
builder.Services.AddScoped<IFundraiserRepository, FundraiserRepository>();
builder.Services.AddScoped<IFundraiserService, FundraiserService>();

// Register FundraiserStatisticsRepository and FundraiserStatisticsService for DI
builder.Services.AddScoped<IFundraiserStatisticsRepository, FundraiserStatisticsRepository>();
builder.Services.AddScoped<IFundraiserStatisticsService, FundraiserStatisticsService>();

builder.Services.AddSingleton<IOrganizationRepository, OrganizationRepository>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();

// Register W9FormService for DI
builder.Services.AddScoped<IW9FormService, W9FormService>();

// Add to the existing service registrations in Program.cs
builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();
builder.Services.AddScoped<ReferralLinkHelper>();

// Register the data seeder
builder.Services.AddScoped<MessageTemplateDataSeeder>();
builder.Services.AddScoped<OrganizationDataSeeder>();
builder.Services.AddScoped<CampaignDataSeeder>();

builder.Services.AddScoped<AdminInitializationService>();

builder.Services.AddSingleton<ICacheStrategy, MemoryCacheStrategy>();

var app = builder.Build();

// Apply pending migrations and create database/tables if needed
using (var scope = app.Services.CreateScope())
{
    // Migrate the unified ApplicationDbContext
    var appDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    appDbContext.Database.Migrate();

    // Initialize settings from appsettings.json
    var settingsInitializer = scope.ServiceProvider.GetRequiredService<SettingsInitializer>();
    await settingsInitializer.InitializeSettingsAsync();

    // Seed organization data
    var organizationSeeder = scope.ServiceProvider.GetRequiredService<OrganizationDataSeeder>();
    await organizationSeeder.SeedAsync();

    // Seed message templates
    var messageTemplateSeeder = scope.ServiceProvider.GetRequiredService<MessageTemplateDataSeeder>();
    await messageTemplateSeeder.SeedAsync();

    var campaignTemplateSeeder = scope.ServiceProvider.GetRequiredService<CampaignDataSeeder>();
    await campaignTemplateSeeder.SeedAsync();
}

using (var scope = app.Services.CreateScope())
{
    var adminInitService = scope.ServiceProvider.GetRequiredService<AdminInitializationService>();
    await adminInitService.InitializeDefaultAdminAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapBlazorHub();
app.MapControllers(); // Add this line to map API controllers
// Update the fallback route to point to your new location
app.MapFallbackToPage("/_Host");

app.Run();
