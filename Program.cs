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

using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Common.Data.Extensions;
using msih.p4g.Server.Common.Data.Repositories;
using msih.p4g.Server.Features.Base.EmailService.Interfaces;
using msih.p4g.Server.Features.Base.EmailService.Services;
using msih.p4g.Server.Features.Base.PaymentService.Extensions;
using msih.p4g.Server.Features.Base.Settings.Interfaces;
using msih.p4g.Server.Features.Base.Settings.Services;
using msih.p4g.Server.Features.Base.SmsService.Extensions;
using msih.p4g.Shared.Models;

var builder = WebApplication.CreateBuilder(args);

//Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.RootDirectory = "/Server/Pages";
});
//builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add Entity Framework with conditional provider selection based on environment
DatabaseConfigurationHelper.AddConfiguredDbContext<ApplicationDbContext>(
    builder.Services,
    builder.Configuration,
    builder.Environment);

// Register generic repository for Setting using ApplicationDbContext
builder.Services.AddScoped<IGenericRepository<Setting>, GenericRepository<Setting, ApplicationDbContext>>();

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

// Register SettingsService for DI
builder.Services.AddScoped<ISettingsService, SettingsService>();



var app = builder.Build();

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
