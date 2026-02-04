using Azure.AI.OpenAI;
using RagWorkshop.Ingestion.Interfaces;
using RagWorkshop.Ingestion.Services;
using RagWorkshop.Repository.Interfaces;
using RagWorkshop.Repository.Services;

namespace RagWorkshop.Api.Extensions;

/// <summary>
/// Extension methods for ingestion services
/// MODULE 1: You'll implement this to wire up the ingestion pipeline
/// </summary>
public static class IngestionServiceExtensions
{
    public static IServiceCollection AddIngestionServices(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO - MODULE 1: Register all ingestion services
        throw new NotImplementedException("AddIngestionServices - to be implemented in Module 1");
    }
}
