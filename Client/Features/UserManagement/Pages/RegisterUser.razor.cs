// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using msih.p4g.Client.Features.Authentication.Services;
using msih.p4g.Server.Features.Base.ProfileService.Model;
using msih.p4g.Server.Features.Base.SettingsService.Interfaces;
using msih.p4g.Server.Features.Base.UserProfileService.Interfaces;
using msih.p4g.Server.Features.Base.UserService.Models;


namespace msih.p4g.Client.Features.UserManagement.Pages
{
    public partial class RegisterUser : ComponentBase
    {
        [Inject]
        private IUserProfileService UserProfileService { get; set; } = null!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = null!;

        [Inject]
        private AuthService AuthService { get; set; } = null!;

        [Inject]
        public IJSRuntime JSRuntime { get; set; } = null!;

        [Inject]
        private ISettingsService _settingsService { get; set; } = null!;

        [Inject]
        private IConfiguration _configuration { get; set; } = null!;

        private User user = new() { Role = UserRole.Fundraiser }; // Default to Fundraiser
        private Profile profile = new();
        private string message = string.Empty;
        private bool isProcessing;

        // Date of birth fields
        private int month = 1;
        private int day = 1;
        private int year = DateTime.Now.Year - 13; // Default to 13 years ago

        private bool isLoggingIn = false;
        private string errorMessage = string.Empty;
        private string successMessage = string.Empty;

        private bool isMarketer => Route switch
        {
            "/student" => true,
            "/influencer" => true,
            _ => false
        };

        private bool isRegistered = false;
        private string referralCode = "";
        private string userName = "";
        private string donationUrl = "https://msih.org/donate"; // Default value

        private string Title => Route switch
        {
            "/student" => "Student Registration",
            "/influencer" => "Influencer Registration",
            _ => "Register"
        };

        private string Description => Route switch
        {
            "/student" => "Sign up as a student and start your journey.",
            "/influencer" => "Sign up as an influencer and earn rewards.",
            _ => ""
        };
        private string Route => NavigationManager.Uri.Replace(NavigationManager.BaseUri, "/").TrimEnd('/');
        private string RouteName => Route switch
        {
            "/student" => "Student",
            "/influencer" => "Influencer",
            _ => "User"
        };

        private UserRole GetRoleForRoute() => Route switch
        {
            "/student" => UserRole.Fundraiser,
            "/influencer" => UserRole.Fundraiser,
            _ => UserRole.Fundraiser
        };

        protected override void OnInitialized()
        {
            // Ensure role is set to Fundraiser
            user.Role = UserRole.Fundraiser;
            base.OnInitialized();
        }

        /// <summary>
        /// Handles errors from the referral link component
        /// </summary>
        private void HandleReferralError(string errorMessage)
        {
            message = errorMessage;
            StateHasChanged();
        }

        private async Task HandleRegistration()
        {
            // Manual validation for first and last name
            var validationErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(profile.FirstName))
                validationErrors.Add("First name is required.");

            if (string.IsNullOrWhiteSpace(profile.LastName))
                validationErrors.Add("Last name is required.");

            if (validationErrors.Any())
            {
                message = "Error: " + string.Join(" ", validationErrors);
                StateHasChanged();
                return;
            }

            // Set the DateOfBirth from the selected month, day, and year
            SetDateOfBirth();

            isProcessing = true;
            message = "";

            isLoggingIn = true;
            errorMessage = string.Empty;
            successMessage = string.Empty;

            try
            {
                // Create both the user and profile in a single operation
                var createdProfile = await UserProfileService.CreateUserWithProfileAsync(user, profile);
                //referralCode = createdProfile.ReferralCode;

                //// Update the donation URL from settings
                //donationUrl = await _settingsService.GetValueAsync("DonationURL")
                //   ?? _configuration["DonationURL"]
                //   ?? "https://msih.org/donate";

                try
                {
                    // Request login email with verification link if needed
                    var (success, messageResult) = await AuthService.RequestLoginEmailAsync(user.Email);

                    if (success)
                    {
                        successMessage = messageResult;
                    }
                    else
                    {
                        errorMessage = messageResult;
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = $"An error occurred: {ex.Message}";
                }
                finally
                {
                    message = $"User registered successfully!";
                    isRegistered = true;
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                message = $"Error: {ex.Message}";
            }
            finally
            {
                isProcessing = false;
                StateHasChanged();
            }
        }

        // Helper method to set the DateOfBirth from month, day, and year
        private void SetDateOfBirth()
        {
            try
            {
                profile.DateOfBirth = new DateTime(year, month, day);
            }
            catch
            {
                // Handle invalid date (like February 30th)
                profile.DateOfBirth = null;
            }
        }
    }
}
