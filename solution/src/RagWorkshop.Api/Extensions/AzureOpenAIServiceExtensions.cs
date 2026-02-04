using Azure;
using Azure.AI.OpenAI;

namespace RagWorkshop.Api.Extensions;

/// <summary>
/// Extension methods for configuring Azure OpenAI services
/// </summary>
public static class AzureOpenAIServiceExtensions
{
    public static IServiceCollection AddAzureOpenAI(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var endpoint = config["AzureOpenAI:Endpoint"];
            var apiKey = config["AzureOpenAI:ApiKey"];

            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
            {
                var logger = sp.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("Azure OpenAI not configured. Add AzureOpenAI:Endpoint and AzureOpenAI:ApiKey to appsettings.json");
                return null!;
            }

            return new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
        });

        return services;
    }
}
