using JwtAuthServiceDemo.Models;

namespace JwtAuthServiceDemo.Services;

public interface IAwsSecretsService
{
    Task<string> GetSecretAsync(string secretName);
    Task<List<ApiKeyModel>> GetApiKeysAsync();
}