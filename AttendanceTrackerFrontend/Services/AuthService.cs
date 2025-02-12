using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;

namespace AttendanceTrackerFrontend.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;

        public CustomAuthStateProvider(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("accessToken");

            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            var identity = GetClaimsFromToken(token);
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }

        /// <summary>
        /// Stores the JWT token in local storage and updates authentication state.
        /// </summary>
        public async Task SetTokenAsync(string token)
        {
            await _localStorage.SetItemAsync("accessToken", token);

            var identity = GetClaimsFromToken(token);
            var user = new ClaimsPrincipal(identity);
            var authState = Task.FromResult(new AuthenticationState(user));

            NotifyAuthenticationStateChanged(authState); // ✅ FIXED: Correctly calls the method with a Task
        }

        /// <summary>
        /// Logs the user out by clearing the token and resetting authentication state.
        /// </summary>
        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync("accessToken");

            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));

            NotifyAuthenticationStateChanged(authState); // ✅ FIXED: Correctly calls the method
        }

        private ClaimsIdentity GetClaimsFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return new ClaimsIdentity(jwt.Claims, "jwt");
        }
    }
}
