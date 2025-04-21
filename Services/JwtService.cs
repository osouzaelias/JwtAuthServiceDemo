using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JwtAuthServiceDemo.Models;
using Microsoft.IdentityModel.Tokens;

namespace JwtAuthServiceDemo.Services;

public class JwtService
    {
        private readonly IConfiguration _configuration;
        private readonly IAwsSecretsService _secretsService;
        private readonly ILogger<JwtService> _logger;

        public JwtService(IConfiguration configuration, IAwsSecretsService secretsService, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _secretsService = secretsService;
            _logger = logger;
        }

        public async Task<string> GenerateTokenAsync(string clientId, IEnumerable<string> roles)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var expiryMinutes = jwtSettings.GetValue<int>("ExpiryInMinutes");
            var issuer = jwtSettings.GetValue<string>("Issuer");
            var audience = jwtSettings.GetValue<string>("Audience");
            
            var secretKey = await _secretsService.GetSecretAsync("jwt-signing-key");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, clientId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Adiciona roles como claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<ApiKeyModel?> ValidateClientCredentialsAsync(string clientId, string clientSecret)
        {
            try
            {
                var apiKeys = await _secretsService.GetApiKeysAsync();
                return apiKeys.FirstOrDefault(k => 
                    k.ClientId == clientId && 
                    k.ClientSecret == clientSecret);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating client credentials");
                return null;
            }
        }
    }