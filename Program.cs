/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

using Microsoft.EntityFrameworkCore;
using msih.p4g.Server.Common.Data.Extensions;
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.Base.EmailService.Interfaces;
using msih.p4g.Server.Features.Base.EmailService.Services;
using msih.p4g.Server.Features.Base.PaymentService.Extensions;
using msih.p4g.Server.Features.Base.PaymentService.Interfaces;
using msih.p4g.Server.Features.Base.PaymentService.Services;
using msih.p4g.Server.Features.Base.ProfileService.Interfaces;
using msih.p4g.Server.Features.Base.ProfileService.Repositories;
using msih.p4g.Server.Features.Base.ProfileService.Services;
using msih.p4g.Server.Features.Base.SettingsService.Extensions;
using msih.p4g.Server.Features.Base.SettingsService.Interfaces;
using msih.p4g.Server.Features.Base.SettingsService.Model; // Add this using for EF Core migrations
using msih.p4g.Server.Features.Base.SettingsService.Services;
using msih.p4g.Server.Features.Base.SmsService.Extensions;
using msih.p4g.Server.Features.Base.UserService.Data;
using msih.p4g.Server.Features.CampaignService.Data;
using msih.p4g.Server.Features.DonorService.Interfaces;
using msih.p4g.Server.Features.DonorService.Services;
using msih.p4g.Server.Features.Base.SmsService.Data;
using msih.p4g.Server.Features.Base.PaymentService.Data;
using msih.p4g.Server.Features.Base.SettingsService.Data;
using msih.p4g.Server.Features.DonorService.Data;
using msih.p4g.Server.Features.Base.ProfileService.Data;
using msih.p4g.Server.Features.DonationService.Data;
using msih.p4g.Server.Features.DonationService.Services;
using msih.p4g.Server.Features.Base.UserService.Interfaces;
using msih.p4g.Server.Features.Base.UserService.Repositories;

var builder = WebApplication.CreateBuilder(args);

//Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.RootDirectory = "/Server/Pages";
});
//builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add Entity Framework with conditional provider selection based on environment
DatabaseConfigurationHelper.AddConfiguredDbContext<CampaignDbContext>(
    builder.Services,
    builder.Configuration,
    builder.Environment);

// Register UserDbContext for DI and migrations
DatabaseConfigurationHelper.AddConfiguredDbContext<UserDbContext>(
    builder.Services,
    builder.Configuration,
    builder.Environment);

// Register SmsDbContext for DI and migrations
DatabaseConfigurationHelper.AddConfiguredDbContext<SmsDbContext>(
    builder.Services,
    builder.Configuration,
    builder.Environment);

// Register PaymentDbContext for DI and migrations
DatabaseConfigurationHelper.AddConfiguredDbContext<PaymentDbContext>(
    builder.Services,
    builder.Configuration,
    builder.Environment);

// Register SettingsDbContext for DI and migrations
DatabaseConfigurationHelper.AddConfiguredDbContext<SettingsDbContext>(
    builder.Services,
    builder.Configuration,
    builder.Environment);

// Register DonorDbContext for DI and migrations
DatabaseConfigurationHelper.AddConfiguredDbContext<DonorDbContext>(
    builder.Services,
    builder.Configuration,
    builder.Environment);

// Register ProfileDbContext for DI and migrations
DatabaseConfigurationHelper.AddConfiguredDbContext<ProfileDbContext>(
    builder.Services,
    builder.Configuration,
    builder.Environment);

// Register DonationDbContext for DI and migrations
DatabaseConfigurationHelper.AddConfiguredDbContext<DonationDbContext>(
    builder.Services,
    builder.Configuration,
    builder.Environment);

// Register generic repository for Setting using SettingsDbContext (not CampaignDbContext)
builder.Services.AddScoped<IGenericRepository<Setting>, GenericRepository<Setting, SettingsDbContext>>();

// Register Email Service - choose one implementation based on configuration or use a factory
string emailProvider = builder.Configuration["EmailProvider"] ?? "SendGrid";
if (emailProvider.Equals("AWS", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<IEmailService, AWSSESEmailService>();
}
else
{
    builder.Services.AddScoped<IEmailService, SendGridEmailService>();
}

// Register SMS Service and related dependencies
builder.Services.AddSmsServices(builder.Configuration, builder.Environment);

// Register Payment Service and related dependencies
builder.Services.AddPaymentServices(builder.Configuration, builder.Environment);

// Register IPaymentService using a factory (resolves dependency injection error)
builder.Services.AddScoped<IPaymentService>(provider => {
    var factory = provider.GetRequiredService<IPaymentServiceFactory>();
    return factory.GetDefaultPaymentService();
});

// Register Settings Service and related dependencies
builder.Services.AddSettingsServices();

// Register DonorService for DI
builder.Services.AddScoped<IDonorService, DonorService>();

// Register ProfileRepository and ProfileService for DI
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IProfileService, ProfileService>();

// Register DonationRepository and DonationService for DI
builder.Services.AddScoped<IDonationRepository, DonationRepository>();
builder.Services.AddScoped<DonationService>();

// Register UserRepository for DI (needed by DonationService)
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

// Apply pending migrations and create database/tables if needed
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CampaignDbContext>();
    dbContext.Database.Migrate();
    // Migrate all other DbContexts
    scope.ServiceProvider.GetRequiredService<UserDbContext>().Database.Migrate();
    scope.ServiceProvider.GetRequiredService<SmsDbContext>().Database.Migrate();
    scope.ServiceProvider.GetRequiredService<PaymentDbContext>().Database.Migrate();
    scope.ServiceProvider.GetRequiredService<SettingsDbContext>().Database.Migrate();
    scope.ServiceProvider.GetRequiredService<DonorDbContext>().Database.Migrate();
    scope.ServiceProvider.GetRequiredService<ProfileDbContext>().Database.Migrate();
    scope.ServiceProvider.GetRequiredService<DonationDbContext>().Database.Migrate();
    
    // Initialize settings from appsettings.json
    var settingsInitializer = scope.ServiceProvider.GetRequiredService<SettingsInitializer>();
    await settingsInitializer.InitializeSettingsAsync();
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
// Update the fallback route to point to your new location
app.MapFallbackToPage("/_Host");

app.Run();
