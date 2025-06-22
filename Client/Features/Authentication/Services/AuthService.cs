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

using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using msih.p4g.Server.Features.Base.UserService.Interfaces;
using msih.p4g.Server.Features.Base.UserService.Models;


namespace msih.p4g.Client.Features.Authentication.Services
{
    public class AuthService
    {
        private readonly ProtectedLocalStorage _localStorage;
        private readonly IUserService _userService;
        private readonly IEmailVerificationService _emailVerificationService;
        private User? _currentUser;

        public event Action? AuthStateChanged;
        public User? CurrentUser => _currentUser;
        public bool IsAuthenticated => _currentUser != null;

        public AuthService(
            ProtectedLocalStorage localStorage,
            IUserService userService,
            IEmailVerificationService emailVerificationService)
        {
            _localStorage = localStorage;
            _userService = userService;
            _emailVerificationService = emailVerificationService;
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

        // Send login email with verification link
        public async Task<(bool success, string message)> RequestLoginEmailAsync(string email)
        {
            try
            {
                var user = await _userService.GetByEmailAsync(email);

                if (user == null)
                {
                    return (false, "User not found. Please register a new account.");
                }

                // Send verification email
                var emailSent = await _emailVerificationService.SendVerificationEmailAsync(user);
                if (emailSent)
                {
                    return (true, "Email verification link sent. Please check your inbox and verify your email before logging in.");
                }
                else
                {
                    return (false, "Failed to send verification email. Please try again later.");
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                return (false, $"An error occurred: {ex.Message}");
            }
        }


        // Login with existing user object (after email verification)
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
            var emailSent = await _userService.LogOutUserByIdAsync(_currentUser.Id);
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
                var user = await _userService.GetByIdAsync(userId);

                if (user != null && user.EmailConfirmed)
                {
                    _currentUser = user;
                    NotifyAuthStateChanged();
                    return true;
                }

                // If email is not confirmed, log them out
                if (user != null && !user.EmailConfirmed)
                {
                    await LogoutAsync();
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
