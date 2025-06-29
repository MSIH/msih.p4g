// /**
//  * Copyright (c) 2025 MSIH LLC. All rights reserved.
//  * This file is developed for Make Sure It Happens Inc.
//  * Unauthorized copying, modification, distribution, or use is prohibited.
//  */

using Microsoft.AspNetCore.Components.Authorization;
using msih.p4g.Server.Features.Base.UserService.Models;
using System.Security.Claims;

namespace msih.p4g.Server.Features.Base.AuthorizationService
{
    public class AuthorizationProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal _user = new(new ClaimsIdentity());

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
            => Task.FromResult(new AuthenticationState(_user));

        public void Login(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Role, user.Role.ToString()) // Convert UserRole enum to string
            };

            _user = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void Logout()
        {
            _user = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

    }
}
