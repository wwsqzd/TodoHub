using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using TodoHub.Main.Core.Common;
using TodoHub.Main.Core.DTOs.Response;
using TodoHub.Main.Core.Entities;
using TodoHub.Main.Core.Interfaces;

namespace TodoHub.Main.Core.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IPasswordService _passwordService;
        private readonly IJwtService _jwtService;
        private readonly IUserRepository _userRepository;

        public GoogleAuthService(IHttpClientFactory httpClientFactory, IPasswordService passwordService, IJwtService jwtService, IUserRepository userRepository)
        {
            _httpClientFactory = httpClientFactory;
            _passwordService = passwordService;
            _jwtService = jwtService;
            _userRepository = userRepository;
        }

        // google auth link
        public string GetGoogleLoginUrl()
        {
            var config = new ConfigurationBuilder()
               .AddUserSecrets<GoogleAuthService>() // reads all keys from secrets
                                                              //.AddJsonFile("appsettings.json", optional: true) 
               .Build();
            var clientId = config["web:client_id"];
            var redirectUri = config.GetSection("web:redirect_uris")?.Get<string[]>()?[0];

            var scope = "openid email profile";
            var state = Guid.NewGuid().ToString();

            return $"https://accounts.google.com/o/oauth2/v2/auth" +
                   $"?client_id={clientId}" +
                   $"&redirect_uri={redirectUri}" +
                   $"&response_type=code" +
                   $"&scope={scope}" +
                   $"&access_type=offline" +
                   $"&prompt=consent" +
                   $"&state={state}";
        }

        // google auth callback
        public async Task<(string token, Result<LoginResponseDTO>)> HandleGoogleCallbackAsync(string code, CancellationToken ct)
        {
            var config = new ConfigurationBuilder()
               .AddUserSecrets<GoogleAuthService>() // reads all keys from secrets
                                                    //.AddJsonFile("appsettings.json", optional: true) 
               .Build();
            var clientId = config["web:client_id"];
            var redirectUri = config.GetSection("web:redirect_uris")?.Get<string[]>()?[0];
            var clientSecret = config["web:client_secret"];


            var client = _httpClientFactory.CreateClient();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["redirect_uri"] = redirectUri,
                ["grant_type"] = "authorization_code"
            });

            var response = await client.PostAsync("https://oauth2.googleapis.com/token", content);
            if (!response.IsSuccessStatusCode) return ("", Result<LoginResponseDTO>.Fail("Error"));

            var payload = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());


            var idToken = payload.GetProperty("id_token").GetString();
            var userInfo = ParseIdToken(idToken);

            var user_from_db = await ResilienceExecutor.WithTimeout(t => _userRepository.GetUserByEmailAsyncRepo(userInfo.Email, t), TimeSpan.FromSeconds(5), ct);

            if (user_from_db  == null)
            {
                await ResilienceExecutor.WithTimeout(t => _userRepository.AddGoogleUserAsyncRepo(userInfo, t), TimeSpan.FromSeconds(5), ct);
                user_from_db = await ResilienceExecutor.WithTimeout(t => _userRepository.GetUserByEmailAsyncRepo(userInfo.Email, t), TimeSpan.FromSeconds(5), ct);
            }

            if (user_from_db.GoogleId == null)
            {
                return ("", Result<LoginResponseDTO>.Fail("The user already exists. Link your Google account."));
            }

            

            var accessToken = GenerateAccessToken(user_from_db);
            var refreshToken = await GenerateRefreshToken(user_from_db, ct);

            return (refreshToken, Result<LoginResponseDTO>.Ok(accessToken));
        }


        private UserGoogleDTO ParseIdToken(string idToken)
        {
            var parts = idToken.Split('.');
            var payload = parts[1];
            var jsonBytes = Convert.FromBase64String(PadBase64(payload));
            var json = Encoding.UTF8.GetString(jsonBytes);
            var element = JsonSerializer.Deserialize<JsonElement>(json);

            return new UserGoogleDTO
            {
                Email = element.GetProperty("email").GetString(),
                GoogleId = element.GetProperty("sub").GetString(),
                Name = element.GetProperty("name").GetString(),
                PictureUrl = element.GetProperty("picture").GetString()
            };
        }

        private string PadBase64(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return base64.Replace('-', '+').Replace('_', '/');
        }


        private LoginResponseDTO? GenerateAccessToken(UserEntity user)
        {
            var token = _jwtService.getJwtToken(user);
            var response = new LoginResponseDTO
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAt = token.ValidTo,
            };
            return response;
        }

        private async Task<string?> GenerateRefreshToken(UserEntity user, CancellationToken ct) 
        {
            return await _passwordService.AddRefreshToken(user.Id, ct);
        }
    }
}
