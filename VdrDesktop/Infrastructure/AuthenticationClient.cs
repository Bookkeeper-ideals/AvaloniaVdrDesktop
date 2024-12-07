using IdentityModel.Client;

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace VdrDesktop.Infrastructure
{
    public class AuthenticationClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _tokenUrl;

        public AuthenticationClient(HttpClient httpClient, string tokenUrl)
        {
            _httpClient = httpClient;
            _tokenUrl = tokenUrl;
        }

        public async Task<string?> AuthenticateAsync(string username, string password)
        {
            // Request a token from the OAuth server
            var tokenResponse = await _httpClient.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = _tokenUrl,
                ClientId = "client_id", // Replace with your client ID
                ClientSecret = "client_secret", // Replace with your client secret
                UserName = username,
                Password = password,
                Scope = "api_scope" // Replace with your required scopes
            });

            if (tokenResponse.IsError)
            {
                Console.WriteLine($"Authentication failed: {tokenResponse.Error}");
                return null;
            }

            Console.WriteLine($"Access Token: {tokenResponse.AccessToken}");
            return tokenResponse.AccessToken;
        }
    }
}
