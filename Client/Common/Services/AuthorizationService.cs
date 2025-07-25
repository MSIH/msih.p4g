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

using Microsoft.AspNetCore.Components;
using msih.p4g.Client.Features.Authentication.Services;
using MSIH.Core.Services.Users.Models;

namespace msih.p4g.Client.Common.Services
{
    /// <summary>
    /// Service for handling role-based authorization checks
    /// </summary>
    public class AuthorizationService
    {
        private readonly AuthService _authService;
        private readonly NavigationManager _navigationManager;

        public AuthorizationService(AuthService authService, NavigationManager navigationManager)
        {
            _authService = authService;
            _navigationManager = navigationManager;
        }

        /// <summary>
        /// Checks if the current user has access to the specified roles
        /// </summary>
        /// <param name="allowedRoles">Array of roles that are allowed access</param>
        /// <returns>True if user has access, false otherwise</returns>
        public async Task<bool> HasAccessAsync(params UserRole[] allowedRoles)
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                return currentUser != null && allowedRoles.Contains(currentUser.Role);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking user access: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks access and redirects if unauthorized
        /// </summary>
        /// <param name="allowedRoles">Array of roles that are allowed access</param>
        /// <param name="redirectUrl">URL to redirect to if unauthorized (defaults to home)</param>
        /// <param name="delaySeconds">Delay before redirect in seconds (defaults to 3)</param>
        /// <returns>True if authorized, false if redirecting</returns>
        public async Task<bool> CheckAccessOrRedirectAsync(UserRole[] allowedRoles, string redirectUrl = "/404")
        {
            if (await HasAccessAsync(allowedRoles))
            {
                return true;
            }

            // Delay before redirect
            _navigationManager.NavigateTo(redirectUrl);
            return false;
        }

        /// <summary>
        /// Checks admin access and redirects to 404 if unauthorized
        /// </summary>
        /// <param name="allowedRoles">Array of roles that are allowed access (defaults to Admin only)</param>
        /// <param name="delaySeconds">Delay before redirect in seconds (defaults to 0)</param>
        /// <returns>True if authorized, false if redirecting</returns>
        public async Task<bool> AdminAccessOnlyAsync()
        {
            // Default to Admin role if no roles specified
            var allowedRoles = new[] { UserRole.Admin };

            if (await HasAccessAsync(allowedRoles))
            {
                return true;
            }

            // Use Task.Run to avoid NavigationException during initialization
            _ = Task.Run(async () =>
            {
                await Task.Delay(100); // Small delay to ensure render cycle completes
                _navigationManager.NavigateTo("/404", forceLoad: true);
            });
            return false;
        }

        /// <summary>
        /// Gets the current user's role
        /// </summary>
        /// <returns>The current user's role or null if not authenticated</returns>
        public async Task<UserRole?> GetCurrentUserRoleAsync()
        {
            try
            {
                var currentUser = await _authService.GetCurrentUserAsync();
                return currentUser?.Role;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting current user role: {ex.Message}");
                return null;
            }
        }
    }
}
