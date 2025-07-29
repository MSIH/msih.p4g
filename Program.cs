/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

// Microsoft and System namespaces
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

// Client services
using msih.p4g.Client.Features.Authentication.Services;

// Server common services
using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Common.Data.Extensions;
using msih.p4g.Server.Common.Interfaces;
using msih.p4g.Server.Common.Services;
using msih.p4g.Server.Common.Utilities;

// Base service extensions
using msih.p4g.Server.Features.Base.AffiliateMonitoringService.Extensions;
using msih.p4g.Server.Features.Base.EmailService.Extensions;
using msih.p4g.Server.Features.Base.MessageService.Data;
using msih.p4g.Server.Features.Base.MessageService.Extensions;
using msih.p4g.Server.Features.Base.PaymentService.Extensions;
using msih.p4g.Server.Features.Base.PaymentService.Interfaces;
using msih.p4g.Server.Features.Base.PayoutService.Extensions;
using msih.p4g.Server.Features.Base.ProfileService.Extensions;
using msih.p4g.Server.Features.Base.SettingsService.Extensions;
using msih.p4g.Server.Features.Base.SettingsService.Services;
using msih.p4g.Server.Features.Base.SmsService.Extensions;
using msih.p4g.Server.Features.Base.UserProfileService.Extensions;
using msih.p4g.Server.Features.Base.UserService.Extensions;
using msih.p4g.Server.Features.Base.UserService.Services;
using msih.p4g.Server.Features.Base.W9FormService.Extensions;

// Domain service extensions
using msih.p4g.Server.Features.CampaignService.Data;
using msih.p4g.Server.Features.CampaignService.Extensions;
using msih.p4g.Server.Features.DonationService.Extensions;
using msih.p4g.Server.Features.DonorService.Extensions;
using msih.p4g.Server.Features.FundraiserService.Extensions;
using msih.p4g.Server.Features.OrganizationService.Data;
using msih.p4g.Server.Features.OrganizationService.Extensions;

var builder = WebApplication.CreateBuilder(args);

// =============================================================================
// CORE SERVICES CONFIGURATION
// =============================================================================

// Add Blazor and web services to the container
builder.Services.AddRazorPages(options =>
{
    options.RootDirectory = "/Server/Pages";
});
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers();

// Add core infrastructure services
builder.Services.AddHttpContextAccessor();
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

// =============================================================================
// BASE SERVICES REGISTRATION
// =============================================================================

// Authentication and authorization services
builder.Services.AddScoped<msih.p4g.Client.Features.Authentication.Services.AuthService>();
builder.Services.AddScoped<msih.p4g.Client.Common.Services.AuthorizationService>();

// Communication services
builder.Services.AddEmailServices(builder.Configuration);
builder.Services.AddSmsServices(builder.Configuration, builder.Environment);
builder.Services.AddMessageServices(builder.Configuration, builder.Environment);

// Payment and financial services
builder.Services.AddPaymentServices(builder.Configuration, builder.Environment);
builder.Services.AddPayoutServices(builder.Configuration, builder.Environment);
builder.Services.AddScoped<IPaymentService>(provider =>
{
    var factory = provider.GetRequiredService<IPaymentServiceFactory>();
    return factory.GetDefaultPaymentService();
});

// Core platform services
builder.Services.AddSettingsServices();
builder.Services.AddAffiliateMonitoringServices();
builder.Services.AddUserServices();
builder.Services.AddUserProfileServices();
builder.Services.AddProfileServices();
builder.Services.AddW9FormServices();

// =============================================================================
// DOMAIN SERVICES REGISTRATION
// =============================================================================

// Business domain services
builder.Services.AddOrganizationServices();
builder.Services.AddCampaignServices();
builder.Services.AddDonationServices(builder.Configuration, builder.Environment);
builder.Services.AddDonorServices();
builder.Services.AddFundraiserServices();

// =============================================================================
// UTILITIES AND INFRASTRUCTURE
// =============================================================================

// Common utilities and infrastructure
builder.Services.AddScoped<ReferralURLGenerator>();
builder.Services.AddScoped<MessageTemplateDataSeeder>();
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
app.MapControllers();
app.MapFallbackToPage("/_Host");

app.Run();
