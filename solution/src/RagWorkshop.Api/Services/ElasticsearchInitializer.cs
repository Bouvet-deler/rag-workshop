using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Options;
using RagWorkshop.Repository.Settings;

namespace RagWorkshop.Api.Services;

/// <summary>
/// Service responsible for initializing Elasticsearch index with proper mappings
/// </summary>
public class ElasticsearchInitializer
{
    private readonly ElasticsearchClient _client;
    private readonly ILogger<ElasticsearchInitializer> _logger;
    private readonly string _indexName;

    public ElasticsearchInitializer(
        ElasticsearchClient client,
        ILogger<ElasticsearchInitializer> logger,
        IOptions<ElasticsearchSettings> options)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _indexName = options?.Value?.DefaultIndex ?? "rag-documents";
    }

    /// <summary>
    /// Initializes the Elasticsearch index with vector search mapping if it doesn't exist
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            // Check if index exists
            var existsResponse = await _client.Indices.ExistsAsync(_indexName);

            if (existsResponse.Exists)
            {
                _logger.LogInformation("Elasticsearch index '{IndexName}' already exists", _indexName);
                return;
            }

            // Create index with mapping for vector search
            var createResponse = await _client.Indices.CreateAsync(_indexName, c => c
                .Mappings(m => m
                    .Properties<RagWorkshop.Models.DocumentChunk>(p => p
                        .Keyword(k => k.Id)
                        .Keyword(k => k.DocumentId)
                        .Text(t => t.Text)
                        .IntegerNumber(i => i.ChunkIndex)
                        .IntegerNumber(i => i.PageNumber)
                        .DenseVector(d => d.Embedding, dv => dv
                            .Dims(1536)  // Azure OpenAI text-embedding-ada-002 dimension
                            .Similarity("cosine"))
                    )
                )
            );

            if (createResponse.IsValidResponse)
            {
                _logger.LogInformation("Successfully created Elasticsearch index '{IndexName}' with vector mapping", _indexName);
            }
            else
            {
                _logger.LogError("Failed to create Elasticsearch index: {Error}",
                    createResponse.ElasticsearchServerError?.Error?.Reason);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing Elasticsearch index");
        }
    }
}
