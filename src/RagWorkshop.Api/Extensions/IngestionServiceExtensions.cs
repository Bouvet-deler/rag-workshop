using Azure.AI.OpenAI;
using RagWorkshop.Ingestion.Interfaces;
using RagWorkshop.Ingestion.Services;
using RagWorkshop.Repository.Interfaces;
using RagWorkshop.Repository.Services;

namespace RagWorkshop.Api.Extensions;

/// <summary>
/// Extension methods for configuring document ingestion services
/// </summary>
public static class IngestionServiceExtensions
{
    public static IServiceCollection AddIngestionServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Repository Services
        services.AddScoped<IDocumentRepository, ElasticsearchDocumentRepository>();

        // Configure Ingestion Services
        services.AddScoped<ITextChunker, SimpleTextChunker>();
        services.AddScoped<IPdfExtractor, PdfExtractor>();

        // Configure Embedding Generator (requires Azure OpenAI)
        services.AddScoped<IEmbeddingGenerator>(sp =>
        {
            var openAIClient = sp.GetService<OpenAIClient>();
            var config = sp.GetRequiredService<IConfiguration>();
            var deploymentName = config["AzureOpenAI:EmbeddingDeploymentName"] ?? "text-embedding-ada-002";

            if (openAIClient == null)
            {
                throw new InvalidOperationException("Azure OpenAI client not configured. Set credentials in appsettings.json");
            }

            return new AzureOpenAIEmbeddingGenerator(openAIClient, deploymentName);
        });

        services.AddScoped<IngestionService>();

        return services;
    }
}
