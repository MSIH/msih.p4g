using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using msih.p4g.Server.Features.Base.ProfileService.Model;
using msih.p4g.Server.Features.Base.UserService.Models;
using msih.p4g.Server.Features.Base.UserProfileService.Interfaces;

namespace msih.p4g.Client.Features.UserManagement.Pages
{
    public partial class RegisterUser : ComponentBase
    {
        [Inject]
        private IUserProfileService UserProfileService { get; set; }
        
        [Inject]
        private NavigationManager NavigationManager { get; set; }
        
        private User user = new() { Role = UserRole.Donor };
        private Profile profile = new();
        private string message;
        private bool isProcessing;
        
        private async Task HandleRegistration()
        {
            isProcessing = true;
            message = "";
            
            try
            {
                // Create both the user and profile in a single operation
                var createdProfile = await UserProfileService.CreateUserWithProfileAsync(user, profile);
                
                message = $"User registered successfully! Referral code: {createdProfile.ReferralCode}";
                
                // Reset the form
                user = new() { Role = UserRole.Donor };
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
    }
}
