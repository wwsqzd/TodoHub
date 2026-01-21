using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TodoHub.Main.Core.Common;
using TodoHub.Main.Core.DTOs.Response;
using TodoHub.Main.Core.Entities;
using TodoHub.Main.Core.Interfaces;

namespace TodoHub.Main.Core.Services
{
    public class GitHubAuthService : IGitHubAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IPasswordService password_serv;
        private readonly IJwtService jwt_serv;
        private readonly IUserRepository user_repo;
        private readonly DbBulkhead _dbBulkhead;

        public GitHubAuthService(
            IHttpClientFactory httpClientFactory,
            IPasswordService passwordService,
            IJwtService jwtService,
            IUserRepository userRepository,
            DbBulkhead dbBulkhead
            )
        {
            _httpClientFactory = httpClientFactory;
            password_serv = passwordService;
            jwt_serv = jwtService;
            user_repo = userRepository;
            _dbBulkhead = dbBulkhead;
        }

        public string GetGitHubLoginUrl()
        {
            var _configuration = new ConfigurationBuilder()
               .AddUserSecrets<GitHubAuthService>() // reads all keys from secrets
                                                    //.AddJsonFile("appsettings.json", optional: true) 
               .Build();
            var clientId = _configuration["github_auth:client_id"];
            var redirectUri = _configuration.GetSection("github_auth:redirect_uris")?.Get<string[]>()?[0];
            var state = Guid.NewGuid().ToString(); // in cache 

           
            return $"https://github.com/login/oauth/authorize" +
                   $"?scope=user:email" + 
                   $"&client_id={clientId}" +
                   $"&redirect_uri={redirectUri}" +
                   $"&state={state}";
        }

        public async Task<(string token, Result<LoginResponseDTO>)> HandleGitHubCallbackAsync(string code, CancellationToken ct)
        {
            var _configuration = new ConfigurationBuilder()
               .AddUserSecrets<GitHubAuthService>() // reads all keys from secrets
                                                    //.AddJsonFile("appsettings.json", optional: true) 
               .Build();
            var clientId = _configuration["github_auth:client_id"];
            var clientSecret = _configuration["github_auth:client_secret"];
            var redirectUri = _configuration.GetSection("github_auth:redirect_uris")?.Get<string[]>()?[0];

            var client = _httpClientFactory.CreateClient();

            
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("TodoHub", "1.0"));

            
            var tokenRequestParams = new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = clientId!,
                ["client_secret"] = clientSecret!,
                ["redirect_uri"] = redirectUri!
            };

            
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token")
            {
                Content = new FormUrlEncodedContent(tokenRequestParams)
            };
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.SendAsync(requestMessage);
            if (!response.IsSuccessStatusCode) return ("", Result<LoginResponseDTO>.Fail("GitHub Auth Failed"));

            var payload = await response.Content.ReadFromJsonAsync<JsonElement>();

            
            if (payload.TryGetProperty("error", out _))
            {
                return ("", Result<LoginResponseDTO>.Fail("Token exchange failed"));
            }

            var accessToken = payload.GetProperty("access_token").GetString();


            var userRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
            userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var userResponse = await client.SendAsync(userRequest);
            if (!userResponse.IsSuccessStatusCode) return ("", Result<LoginResponseDTO>.Fail("Failed to get user info"));

            var githubUserJson = await userResponse.Content.ReadFromJsonAsync<JsonElement>();

            
            var userInfo = new UserGitHubDTO
            {
                GitHubId = githubUserJson.GetProperty("id").ToString(),
                Name = githubUserJson.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : githubUserJson.GetProperty("login").GetString(),
                PictureUrl = githubUserJson.GetProperty("avatar_url").GetString(),
                Email = githubUserJson.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null
            };


            if (string.IsNullOrEmpty(userInfo.Email))
            {
                using var emailRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user/emails");
                emailRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                using var emailResponse = await client.SendAsync(emailRequest);

                if (emailResponse.IsSuccessStatusCode)
                {
                    var emailsJson = await emailResponse.Content.ReadFromJsonAsync<JsonElement>();

                    
                    foreach (var emailObj in emailsJson.EnumerateArray())
                    {
                        var isPrimary = emailObj.GetProperty("primary").GetBoolean();
                        var isVerified = emailObj.GetProperty("verified").GetBoolean();

                        if (isPrimary && isVerified)
                        {
                            userInfo.Email = emailObj.GetProperty("email").GetString();
                            break; 
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(userInfo.Email))
            {
                return ("", Result<LoginResponseDTO>.Fail("No verified email found on GitHub account."));
            }

            
            var userFromDb = await _dbBulkhead.ExecuteAsync(bct => ResilienceExecutor.WithTimeout(t => user_repo.GetUserByEmailAsyncRepo(userInfo.Email, t), TimeSpan.FromSeconds(5), bct), ct);

            if (userFromDb == null)
            {
                await ResilienceExecutor.WithTimeout(t => user_repo.AddGitHubUserAsyncRepo(userInfo, t), TimeSpan.FromSeconds(5), ct);
                userFromDb = await _dbBulkhead.ExecuteAsync(bct => ResilienceExecutor.WithTimeout(t => user_repo.GetUserByEmailAsyncRepo(userInfo.Email, t), TimeSpan.FromSeconds(5), bct), ct);
            }


            if (userFromDb?.GitHubId == null)
            {
                return ("", Result<LoginResponseDTO>.Fail("User exists but GitHub is not linked."));
            }

            var jwtAccessToken = GenerateAccessToken(userFromDb);
            var refreshToken = await GenerateRefreshToken(userFromDb, ct);

            return (refreshToken, Result<LoginResponseDTO>.Ok(jwtAccessToken));
        }

        private LoginResponseDTO? GenerateAccessToken(UserEntity user)
        {
            var token = jwt_serv.getJwtToken(user);
            return new LoginResponseDTO
            {
                Token = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAt = token.ValidTo,
            };
        }

        private async Task<string?> GenerateRefreshToken(UserEntity user, CancellationToken ct)
        {
            return await password_serv.AddRefreshToken(user.Id, ct);
        }
    }
}
