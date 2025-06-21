/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */
using Microsoft.AspNetCore.Components;
using msih.p4g.Server.Features.OrganizationService.Interfaces;
using msih.p4g.Server.Features.OrganizationService.Models;
using msih.p4g.Shared.OrganizationService.Dtos;
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
        public string Id { get; set; }
        
        [Inject]
        private IOrganizationService OrganizationService { get; set; }
        
        [Inject]
        private NavigationManager NavigationManager { get; set; }
        
        private OrganizationDto OrganizationDto { get; set; } = new();
        private bool IsLoading { get; set; } = true;
        private bool IsNewOrganization => string.IsNullOrEmpty(Id) || Id.ToLower() == "new";
        private string ErrorMessage { get; set; }
        private string SuccessMessage { get; set; }
        
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
                        OrganizationDto = OrganizationDto.FromEntity(organization);
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
                    OrganizationDto = new OrganizationDto
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
                
                var organization = OrganizationDto.ToEntity();
                
                if (IsNewOrganization)
                {
                    await OrganizationService.AddAsync(organization);
                    SuccessMessage = "Organization created successfully.";
                }
                else
                {
                    await OrganizationService.UpdateAsync(organization);
                    SuccessMessage = "Organization updated successfully.";
                }
                
                // Update the DTO with the updated entity
                OrganizationDto = OrganizationDto.FromEntity(organization);
                
                // If it's a new organization, redirect to the edit page for the new ID
                if (IsNewOrganization)
                {
                    NavigationManager.NavigateTo($"admin/organizations/edit/{organization.Id}");
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
