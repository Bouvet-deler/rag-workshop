using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using RagWorkshop.Repository.Settings;

namespace RagWorkshop.Api.Extensions;

/// <summary>
/// Extension methods for Elasticsearch configuration
/// MODULE 0: You'll implement this to connect to Elasticsearch
/// </summary>
public static class ElasticsearchServiceExtensions
{
    public static IServiceCollection AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO - MODULE 0: Configure Elasticsearch client
        throw new NotImplementedException("AddElasticsearch - to be implemented in Module 0");
    }
}
