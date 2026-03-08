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
    // Bind Elasticsearch settings from configuration
    var elasticsearchSettings = configuration.GetSection("Elasticsearch").Get<ElasticsearchSettings>()
        ?? new ElasticsearchSettings();

    services.Configure<ElasticsearchSettings>(configuration.GetSection("Elasticsearch"));

    // Create Elasticsearch client
    var settings = new ElasticsearchClientSettings(new Uri(elasticsearchSettings.Url))
        .DisableDirectStreaming() // Helpful for debugging
        .RequestTimeout(TimeSpan.FromSeconds(60));

    var client = new ElasticsearchClient(settings);
    services.AddSingleton(client);

    return services;
}
}
