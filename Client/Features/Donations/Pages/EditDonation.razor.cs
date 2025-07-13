/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

using Microsoft.AspNetCore.Components;
using msih.p4g.Client.Features.Authentication.Services;
using msih.p4g.Server.Features.DonationService.Interfaces;
using msih.p4g.Server.Features.DonationService.Models;
using msih.p4g.Server.Features.CampaignService.Interfaces;
using msih.p4g.Server.Features.CampaignService.Model;
using msih.p4g.Shared.Dtos;
using System.ComponentModel.DataAnnotations;

namespace msih.p4g.Client.Features.Donations.Pages
{
    /// <summary>
    /// Page component for editing individual donations
    /// </summary>
    public partial class EditDonation
    {
        [Parameter]
        public int Id { get; set; }

        [Inject]
        private IDonationService DonationService { get; set; } = default!;

        [Inject]
        private ICampaignService CampaignService { get; set; } = default!;

        [Inject]
        private AuthService AuthService { get; set; } = default!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        private IJSRuntime JSRuntime { get; set; } = default!;

        private DonationDto? DonationDto { get; set; }
        private List<Campaign> Campaigns { get; set; } = new();
        private EditDonationModel EditModel { get; set; } = new();
        private bool IsLoading { get; set; } = true;
        private bool IsProcessing { get; set; } = false;
        private string? ErrorMessage { get; set; }
        private string? SuccessMessage { get; set; }

        /// <summary>
        /// Determines if this is a recurring donation that can be edited
        /// </summary>
        private bool IsRecurringDonation => DonationDto?.IsMonthly == true || DonationDto?.IsAnnual == true;

        /// <summary>
        /// Initializes the component
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                SuccessMessage = null;

                // Check if user is authenticated
                if (!AuthService.IsAuthenticated || string.IsNullOrEmpty(AuthService.CurrentUser?.Email))
                {
                    ErrorMessage = "You must be logged in to view donations.";
                    NavigationManager.NavigateTo("/login");
                    return;
                }

                // Load campaigns first for name resolution
                await LoadCampaigns();

                // Load the specific donation
                await LoadDonation();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading donation: {ex.Message}";
                Console.WriteLine($"Error in EditDonation.OnInitializedAsync: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Loads all campaigns for name resolution
        /// </summary>
        private async Task LoadCampaigns()
        {
            try
            {
                var allCampaigns = await CampaignService.GetAllAsync();
                Campaigns = allCampaigns.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading campaigns: {ex.Message}");
                Campaigns = new List<Campaign>();
            }
        }

        /// <summary>
        /// Loads the donation details
        /// </summary>
        private async Task LoadDonation()
        {
            try
            {
                // Get the donation by ID
                var donation = await DonationService.GetByIdAsync(Id);

                if (donation == null)
                {
                    ErrorMessage = "Donation not found.";
                    return;
                }

                // Verify that this donation belongs to the current user
                var userDonations = await DonationService.GetByUserEmailAsync(AuthService.CurrentUser!.Email!);
                if (!userDonations.Any(d => d.Id == Id))
                {
                    ErrorMessage = "You don't have permission to view this donation.";
                    return;
                }

                // Convert to DTO with campaign name resolution
                DonationDto = ConvertToDto(donation);

                // Initialize edit model
                EditModel = new EditDonationModel
                {
                    DonationAmount = DonationDto.DonationAmount,
                    IsActive = DonationDto.IsActive
                };
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading donation: {ex.Message}";
                Console.WriteLine($"Error in LoadDonation: {ex}");
            }
        }

        /// <summary>
        /// Converts server donation model to DTO with campaign name
        /// </summary>
        private DonationDto ConvertToDto(Donation donation)
        {
            return new DonationDto
            {
                Id = donation.Id,
                DonationAmount = donation.DonationAmount,
                PayTransactionFee = donation.PayTransactionFee,
                PayTransactionFeeAmount = donation.PayTransactionFeeAmount,
                IsMonthly = donation.IsMonthly,
                IsAnnual = donation.IsAnnual,
                DonationMessage = donation.DonationMessage,
                ReferralCode = donation.ReferralCode,
                CampaignCode = donation.CampaignCode,
                CampaignName = GetCampaignName(donation.CampaignId, donation.CampaignCode),
                CreatedOn = donation.CreatedOn,
                IsActive = donation.IsActive
            };
        }

        /// <summary>
        /// Gets the campaign name from ID or code
        /// </summary>
        private string? GetCampaignName(int? campaignId, string? campaignCode)
        {
            // First try to find by CampaignId if available
            if (campaignId.HasValue)
            {
                var campaignById = Campaigns.FirstOrDefault(c => c.Id == campaignId.Value);
                if (campaignById != null)
                    return campaignById.Title;
            }

            // If no CampaignId or not found, try to parse CampaignCode as ID
            if (!string.IsNullOrEmpty(campaignCode) && int.TryParse(campaignCode, out int codeAsId))
            {
                var campaignByCode = Campaigns.FirstOrDefault(c => c.Id == codeAsId);
                if (campaignByCode != null)
                    return campaignByCode.Title;
            }

            // Return the campaign code as fallback if no campaign found
            return campaignCode;
        }

        /// <summary>
        /// Saves changes to the donation
        /// </summary>
        private async Task SaveChanges()
        {
            if (DonationDto == null || !IsRecurringDonation)
                return;

            try
            {
                IsProcessing = true;
                ErrorMessage = null;
                SuccessMessage = null;
                StateHasChanged();

                // Update the recurring donation
                var success = await DonationService.UpdateRecurringDonationAsync(
                    AuthService.CurrentUser!.Email!,
                    DonationDto.Id,
                    EditModel.DonationAmount,
                    EditModel.IsActive);

                if (success)
                {
                    // Update the local donation object
                    DonationDto.DonationAmount = EditModel.DonationAmount;
                    DonationDto.IsActive = EditModel.IsActive;

                    SuccessMessage = "Donation updated successfully!";
                    StateHasChanged();

                    // Optionally navigate back after a short delay
                    await Task.Delay(2000);
                    NavigateBack();
                }
                else
                {
                    ErrorMessage = "Failed to update donation. Please try again.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving donation: {ex.Message}";
                Console.WriteLine($"Error in SaveChanges: {ex}");
            }
            finally
            {
                IsProcessing = false;
                StateHasChanged();
            }
        }

        /// <summary>
        /// Cancels the recurring donation
        /// </summary>
        private async Task CancelDonation()
        {
            if (DonationDto == null || !IsRecurringDonation)
                return;

            try
            {
                var confirmed = await JSRuntime.InvokeAsync<bool>("confirm",
                    $"Are you sure you want to cancel this {DonationDto.RecurrenceType.ToLower()} donation? This action cannot be undone.");

                if (confirmed)
                {
                    IsProcessing = true;
                    ErrorMessage = null;
                    SuccessMessage = null;
                    StateHasChanged();

                    var success = await DonationService.CancelRecurringDonationAsync(
                        AuthService.CurrentUser!.Email!,
                        DonationDto.Id);

                    if (success)
                    {
                        SuccessMessage = "Recurring donation cancelled successfully!";
                        
                        // Update local state
                        DonationDto.IsActive = false;
                        EditModel.IsActive = false;
                        
                        StateHasChanged();

                        // Navigate back after showing success message
                        await Task.Delay(2000);
                        NavigateBack();
                    }
                    else
                    {
                        ErrorMessage = "Failed to cancel donation. Please try again.";
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error canceling donation: {ex.Message}";
                Console.WriteLine($"Error in CancelDonation: {ex}");
            }
            finally
            {
                IsProcessing = false;
                StateHasChanged();
            }
        }

        /// <summary>
        /// Navigates back to the donations list
        /// </summary>
        private void NavigateBack()
        {
            NavigationManager.NavigateTo("/donations");
        }

        /// <summary>
        /// Model for editing donation details
        /// </summary>
        private class EditDonationModel
        {
            [Required]
            [Range(1, double.MaxValue, ErrorMessage = "Donation amount must be greater than $0")]
            public decimal DonationAmount { get; set; }

            public bool IsActive { get; set; }
        }
    }
}