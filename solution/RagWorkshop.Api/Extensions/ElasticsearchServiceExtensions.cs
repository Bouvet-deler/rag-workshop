using Elastic.Clients.Elasticsearch;
using RagWorkshop.Api.Services;
using RagWorkshop.Repository.Settings;

namespace RagWorkshop.Api.Extensions;

/// <summary>
/// Extension methods for configuring Elasticsearch services
/// </summary>
public static class ElasticsearchServiceExtensions
{
    public static IServiceCollection AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Elasticsearch settings
        services.Configure<ElasticsearchSettings>(
            configuration.GetSection("Elasticsearch"));

        // Configure Elasticsearch client
        services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var elasticUrl = config["Elasticsearch:Url"] ?? "http://localhost:9200";
            var defaultIndex = config["Elasticsearch:DefaultIndex"] ?? "rag-documents";
            var settings = new ElasticsearchClientSettings(new Uri(elasticUrl))
                .DefaultIndex(defaultIndex);
            return new ElasticsearchClient(settings);
        });

        // Configure Elasticsearch Initializer
        services.AddScoped<ElasticsearchInitializer>();

        return services;
    }
}
