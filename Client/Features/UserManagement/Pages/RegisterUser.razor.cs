// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using msih.p4g.Client.Features.Authentication.Services;
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

        [Inject]
        private AuthService AuthService { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        private User user = new() { Role = UserRole.Fundraiser }; // Default to Fundraiser
        private Profile profile = new();
        private string message;
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
        private bool appendName = false;
        private string userName = "";

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

        private string ReferralLink => appendName && !string.IsNullOrEmpty(userName)
        ? $"https://gd4.org/give/{referralCode}-{userName.Replace(" ", "")}"
        : $"https://gd4.org/give/{referralCode}";

        private string InstagramUrl => $"https://www.instagram.com/?url={Uri.EscapeDataString(ReferralLink)}";
        private string TikTokUrl => $"https://www.tiktok.com/share?url={Uri.EscapeDataString(ReferralLink)}";
        private string FacebookUrl => $"https://www.facebook.com/sharer/sharer.php?u={Uri.EscapeDataString(ReferralLink)}";
        private string TwitterUrl => $"https://twitter.com/intent/tweet?url={Uri.EscapeDataString(ReferralLink)}";

        bool copyUrlSuccess = false;

        private async Task CopyReferralUrl()
        {
            try
            {
                string referralUrl = $"https://gd4.org/give/{ReferralLink}";
                await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", referralUrl);
                copyUrlSuccess = true;
                StateHasChanged();

                // Reset the success message after 3 seconds
                await Task.Delay(3000);
                copyUrlSuccess = false;
                StateHasChanged();
            }
            catch (Exception)
            {
                // Handle clipboard error
                message = "Unable to copy to clipboard. Please select and copy the URL manually.";
            }
        }



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

            isLoggingIn = true;
            errorMessage = string.Empty;
            successMessage = string.Empty;

            try
            {
                // Create both the user and profile in a single operation
                var createdProfile = await UserProfileService.CreateUserWithProfileAsync(user, profile);
                referralCode = createdProfile.ReferralCode;

                message = $"User registered successfully!";
                isRegistered = true;
                StateHasChanged();

                // Reset the form
                //user = new() { Role = UserRole.Fundraiser };
                //profile = new();

                //try
                //{
                //    // Request login email with verification link if needed
                //    var (success, message) = await AuthService.RequestLoginEmailAsync(createdProfile.User.Email);

                //    if (success)
                //    {
                //        successMessage = message;
                //    }
                //    else
                //    {
                //        errorMessage = message;
                //    }
                //}
                //catch (Exception ex)
                //{
                //    errorMessage = $"An error occurred: {ex.Message}";
                //}
                //finally
                //{
                //    isLoggingIn = false;
                //    StateHasChanged();
                //    // wait two seconds
                //    await Task.Delay(4000);

                //    // If login was successful, we could redirect the user here
                //    if (!string.IsNullOrEmpty(successMessage))
                //    {
                //        // No immediate redirect for now, letting user see the message
                //        NavigationManager.NavigateTo("/verify-email");
                //    }
                //}


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
