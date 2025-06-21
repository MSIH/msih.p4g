/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.AspNetCore.Components;
using msih.p4g.Server.Features.OrganizationService.Interfaces;
using msih.p4g.Server.Features.OrganizationService.Models;
using System;
using System.Threading.Tasks;

namespace msih.p4g.Client.Features.Admin.Pages
{
    /// <summary>
    /// Page component for editing organizations
    /// </summary>
    public partial class OrganizationEditor
    {
        [Parameter]
        public string? Id { get; set; }
        
        [Inject]
        private IOrganizationService OrganizationService { get; set; } = default!;
        
        [Inject]
        private NavigationManager NavigationManager { get; set; } = default!;
        
        private Organization Organization { get; set; } = new();
        private bool IsLoading { get; set; } = true;
        private bool IsNewOrganization => string.IsNullOrEmpty(Id) || Id.ToLower() == "new";
        private string? ErrorMessage { get; set; }
        private string? SuccessMessage { get; set; }
        
        /// <summary>
        /// Initializes the component
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            try
            {
                IsLoading = true;
                
                if (!IsNewOrganization && int.TryParse(Id, out int orgId))
                {
                    var organization = await OrganizationService.GetByIdAsync(orgId);
                    if (organization != null)
                    {
                        Organization = organization;
                    }
                    else
                    {
                        ErrorMessage = "Organization not found.";
                        NavigationManager.NavigateTo("admin/organizations");
                    }
                }
                else
                {
                    // Initialize a new organization
                    Organization = new Organization
                    {
                        IsActive = true,
                        OrganizationType = "501(c)(3)"
                    };
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading organization: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        /// <summary>
        /// Saves the organization
        /// </summary>
        private async Task SaveOrganization()
        {
            try
            {
                ErrorMessage = null;
                SuccessMessage = null;
                
                Organization result;
                if (IsNewOrganization)
                {
                    // Add new organization
                    result = await OrganizationService.AddAsync(Organization);
                    SuccessMessage = "Organization created successfully.";
                    
                    // Update the model with the new ID
                    Organization = result;
                    
                    // Redirect to the edit page for the new ID
                    NavigationManager.NavigateTo($"admin/organizations/edit/{result.Id}");
                }
                else
                {
                    // Update existing organization
                    result = await OrganizationService.UpdateAsync(Organization);
                    SuccessMessage = "Organization updated successfully.";
                    
                    // Update the model with any changes from the service
                    Organization = result;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving organization: {ex.Message}";
            }
        }
        
        /// <summary>
        /// Cancels editing and returns to the organization list
        /// </summary>
        private void Cancel()
        {
            NavigationManager.NavigateTo("admin/organizations");
        }
    }
}
