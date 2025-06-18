/**
 * Copyright (c) 2025 MSIH LLC. All rights reserved.
 * This file is developed for Make Sure It Happens Inc.
 * Unauthorized copying, modification, distribution, or use is prohibited.
 */

using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using msih.p4g.Server.Features.Base.UserService.Models;
using msih.p4g.Server.Features.Base.UserService.Interfaces;
using System;
using System.Threading.Tasks;

namespace msih.p4g.Client.Features.Authentication.Services
{
    public class AuthService
    {
        private readonly ProtectedLocalStorage _localStorage;
        private readonly IUserService _userService;
        private User? _currentUser;
        
        public event Action? AuthStateChanged;
        public User? CurrentUser => _currentUser;
        public bool IsAuthenticated => _currentUser != null;

        public AuthService(
            ProtectedLocalStorage localStorage,
            IUserService userService)
        {
            _localStorage = localStorage;
            _userService = userService;
        }

        public async Task InitializeAuthenticationStateAsync()
        {
            try
            {
                // Try to get the stored user ID from browser storage
                var result = await _localStorage.GetAsync<int>("userId");
                
                if (result.Success)
                {
                    var userId = result.Value;
                    await GetUserByIdAsync(userId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing auth state: {ex.Message}");
                // Continue without authentication if there's an error
            }
        }

        // Login with email only (no password required for MVP)
        public async Task<bool> LoginAsync(string email)
        {
            try
            {
                var user = await _userService.GetByEmailAsync(email);
                
                if (user != null)
                {
                    await _localStorage.SetAsync("userId", user.Id);
                    _currentUser = user;
                    NotifyAuthStateChanged();
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                return false;
            }
        }

        // Login with an existing user object (e.g., for testing)
        public async Task LoginAsync(User user)
        {
            if (user != null)
            {
                await _localStorage.SetAsync("userId", user.Id);
                _currentUser = user;
                NotifyAuthStateChanged();
            }
        }

        public async Task LogoutAsync()
        {
            await _localStorage.DeleteAsync("userId");
            _currentUser = null;
            NotifyAuthStateChanged();
        }

        private void NotifyAuthStateChanged()
        {
            AuthStateChanged?.Invoke();
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            if (_currentUser == null)
            {
                await InitializeAuthenticationStateAsync();
            }
            
            return _currentUser;
        }

        private async Task<bool> GetUserByIdAsync(int userId)
        {
            try
            {
                // This will use the generic repository's GetByIdAsync method
                // through the UserService
                var user = await _userService.GetByIdAsync(userId);
                
                if (user != null)
                {
                    _currentUser = user;
                    NotifyAuthStateChanged();
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user by ID: {ex.Message}");
                return false;
            }
        }
    }
}
