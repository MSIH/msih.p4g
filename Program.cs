
﻿using msih.p4g.Server.Common.Data;
using msih.p4g.Server.Common.Data.Extensions;
using msih.p4g.Server.Features.Base.EmailService.Interfaces;
using msih.p4g.Server.Features.Base.EmailService.Services;
using msih.p4g.Server.Features.Base.SmsService.Extensions;
using msih.p4g.Server.Features.Base.Settings.Interfaces;
using msih.p4g.Server.Features.Base.Settings.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add Entity Framework with conditional provider selection based on environment
DatabaseConfigurationHelper.AddConfiguredDbContext<ApplicationDbContext>(
    builder.Services,
    builder.Configuration,
    builder.Environment);

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
app.MapFallbackToPage("/_Host");

app.Run();