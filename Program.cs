using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Amazon;
using Amazon.Runtime;
using Amazon.SecretsManager;
using JwtAuthServiceDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuração do LocalStack para AWS Secrets Manager
var useLocalStack = builder.Configuration.GetValue<bool>("AWS:UseLocalStack", true);

if (useLocalStack)
{
    // Configuração para LocalStack
    var awsOptions = new Amazon.Extensions.NETCore.Setup.AWSOptions
    {
        Region = RegionEndpoint.USEast1,
        Credentials = new BasicAWSCredentials("test", "test"),
    };

    // Define o endpoint do LocalStack para o Secrets Manager
    var serviceUrl = builder.Configuration.GetValue<string>("AWS:LocalStackUrl", "http://localhost:4566");
    awsOptions.DefaultClientConfig.ServiceURL = serviceUrl;

    // Registra o cliente do Secrets Manager com as configurações do LocalStack
    builder.Services.AddSingleton<IAmazonSecretsManager>(sp => new AmazonSecretsManagerClient(
        awsOptions.Credentials, 
        new AmazonSecretsManagerConfig { 
            ServiceURL = serviceUrl, 
            UseHttp = true,
            AuthenticationRegion = awsOptions.Region.SystemName
        }
    ));
}
else
{
    // Configuração normal para AWS Cloud
    builder.Services.AddAWSService<IAmazonSecretsManager>();
}

builder.Services.AddScoped<IAwsSecretsService, AwsSecretsService>();
builder.Services.AddScoped<JwtService>();

// O resto da configuração continua igual...
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var serviceProvider = builder.Services.BuildServiceProvider();
var secretsService = serviceProvider.GetRequiredService<IAwsSecretsService>();
var secretKey = await secretsService.GetSecretAsync("jwt-signing-key");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
