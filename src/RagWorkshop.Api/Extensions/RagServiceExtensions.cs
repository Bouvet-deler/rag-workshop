using Azure.AI.OpenAI;
using RagWorkshop.Rag.Interfaces;
using RagWorkshop.Rag.Services;
using RagWorkshop.Repository.Interfaces;

namespace RagWorkshop.Api.Extensions;

/// <summary>
/// Extension methods for configuring RAG services
/// </summary>
public static class RagServiceExtensions
{
    public static IServiceCollection AddRagServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IRagService>(sp =>
        {
            var openAIClient = sp.GetService<OpenAIClient>();
            var documentRepository = sp.GetRequiredService<IDocumentRepository>();
            var config = sp.GetRequiredService<IConfiguration>();
            var embeddingDeploymentName = config["AzureOpenAI:EmbeddingDeploymentName"] ?? "text-embedding-small-003";
            var chatDeploymentName = config["AzureOpenAI:DeploymentName"] ?? "gpt-4o-mini";

            return new RagService(documentRepository, openAIClient, embeddingDeploymentName, chatDeploymentName);
        });

        return services;
    }
}
