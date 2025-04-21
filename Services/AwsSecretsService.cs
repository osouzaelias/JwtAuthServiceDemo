using System.Text.Json;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Extensions.Caching;
using Amazon.SecretsManager.Model;
using JwtAuthServiceDemo.Models;

namespace JwtAuthServiceDemo.Services;

public class AwsSecretsService : IAwsSecretsService
{
    private readonly IAmazonSecretsManager _secretsManager;
    private readonly SecretsManagerCache _cache;
    private readonly ILogger<AwsSecretsService> _logger;
    private readonly IConfiguration _configuration;

    public AwsSecretsService(
        IAmazonSecretsManager secretsManager,
        ILogger<AwsSecretsService> logger,
        IConfiguration configuration)
    {
        _secretsManager = secretsManager;
        _logger = logger;
        _configuration = configuration;
        _cache = new SecretsManagerCache(secretsManager);
    }

    public async Task<string> GetSecretAsync(string secretName)
    {
        try
        {
            var secretString = await _cache.GetSecretString(secretName);
            _logger.LogInformation($"Retrieved secret {secretName} (might be from cache)");
            return secretString;
        }
        catch (ResourceNotFoundException)
        {
            _logger.LogError($"Secret {secretName} not found");

            // Para ambiente de desenvolvimento com LocalStack, podemos retornar valores padrão
            if (_configuration.GetValue<bool>("AWS:UseLocalStack", false))
            {
                _logger.LogWarning($"Using default values for secret {secretName} in LocalStack environment");

                if (secretName == "jwt-signing-key")
                {
                    return "DevelopmentSigningKey_ForLocalStackOnly!!!!";
                }
                else if (secretName == "backend-api-keys")
                {
                    return @"[
                            {
                                ""clientId"": ""dev-backend"",
                                ""clientSecret"": ""dev-secret"",
                                ""roles"": [""Service"", ""Admin"", ""DataReader""]
                            }
                        ]";
                }
            }

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving secret {secretName}");
            throw;
        }
    }

    public async Task<List<ApiKeyModel>> GetApiKeysAsync()
    {
        var apiKeysJson = await GetSecretAsync("backend-api-keys");
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        return JsonSerializer.Deserialize<List<ApiKeyModel>>(apiKeysJson, options) ?? new List<ApiKeyModel>();
    }
}