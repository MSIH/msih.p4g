// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using Microsoft.AspNetCore.Components;
using msih.p4g.Server.Features.Base.ProfileService.Model;
using msih.p4g.Server.Features.Base.UserProfileService.Interfaces;
using msih.p4g.Server.Features.Base.UserService.Models;

namespace msih.p4g.Client.Features.UserManagement.Pages
{
    public partial class RegisterUser : ComponentBase
    {
        [Inject]
        private IUserProfileService UserProfileService { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        private User user = new() { Role = UserRole.Fundraiser }; // Default to Fundraiser
        private Profile profile = new();
        private string message;
        private bool isProcessing;

        // Date of birth fields
        private int month = 1;
        private int day = 1;
        private int year = DateTime.Now.Year - 13; // Default to 13 years ago



        protected override void OnInitialized()
        {
            // Ensure role is set to Fundraiser
            user.Role = UserRole.Fundraiser;
            base.OnInitialized();
        }

        private async Task HandleRegistration()
        {
            // Set the DateOfBirth from the selected month, day, and year
            SetDateOfBirth();

            isProcessing = true;
            message = "";

            try
            {
                // Create both the user and profile in a single operation
                var createdProfile = await UserProfileService.CreateUserWithProfileAsync(user, profile);

                message = $"User registered successfully! Referral code: {createdProfile.ReferralCode}";

                // Reset the form
                user = new() { Role = UserRole.Fundraiser };
                profile = new();

                // Navigate after a short delay
                await Task.Delay(2000);
                NavigationManager.NavigateTo("/");
            }
            catch (Exception ex)
            {
                message = $"Error: {ex.Message}";
            }
            finally
            {
                isProcessing = false;
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
