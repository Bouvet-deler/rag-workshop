using Azure;
using Azure.AI.OpenAI;

namespace RagWorkshop.Api.Extensions;

public static class AzureOpenAIServiceExtensions
{
    public static IServiceCollection AddAzureOpenAI(this IServiceCollection services, IConfiguration configuration)
    {
        var endpoint = configuration["AzureOpenAI:Endpoint"];
        var apiKey = configuration["AzureOpenAI:ApiKey"];

        if (!string.IsNullOrWhiteSpace(endpoint) && !string.IsNullOrWhiteSpace(apiKey))
        {
            var client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
            services.AddSingleton(client);
        }
        else
        {
            // Register a null client if not configured (for testing without Azure OpenAI)
            services.AddSingleton<OpenAIClient>(provider => null!);
        }

        return services;
    }
}