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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace msih.p4g.Client.Features.Admin.Pages
{
    /// <summary>
    /// Page component for managing organizations
    /// </summary>
    public partial class OrganizationManager
    {
        [Inject]
        private IOrganizationService OrganizationService { get; set; }
        
        [Inject]
        private NavigationManager NavigationManager { get; set; }
        
        private List<OrganizationDto> Organizations { get; set; } = new List<OrganizationDto>();
        private bool IsLoading { get; set; } = true;
        private string ErrorMessage { get; set; }
        
        /// <summary>
        /// Initializes the component
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            try
            {
                IsLoading = true;
                var organizations = await OrganizationService.GetAllAsync();
                Organizations = organizations.Select(OrganizationDto.FromEntity).ToList();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading organizations: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        /// <summary>
        /// Navigates to the organization editor
        /// </summary>
        private void AddOrganization()
        {
            NavigationManager.NavigateTo("admin/organizations/new");
        }
        
        /// <summary>
        /// Navigates to the organization editor for editing
        /// </summary>
        /// <param name="id">The organization ID</param>
        private void EditOrganization(int id)
        {
            NavigationManager.NavigateTo($"admin/organizations/edit/{id}");
        }
        
        /// <summary>
        /// Sets an organization's active status
        /// </summary>
        /// <param name="id">The organization ID</param>
        /// <param name="isActive">The new active status</param>
        private async Task SetActiveStatus(int id, bool isActive)
        {
            try
            {
                await OrganizationService.SetActiveStatusAsync(id, isActive);
                var organization = Organizations.First(o => o.Id == id);
                organization.IsActive = isActive;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating organization status: {ex.Message}";
            }
        }
    }
}
