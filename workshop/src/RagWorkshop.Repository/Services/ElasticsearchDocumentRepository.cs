using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Options;
using RagWorkshop.Models;
using RagWorkshop.Repository.Interfaces;
using RagWorkshop.Repository.Settings;

namespace RagWorkshop.Repository.Services;

/// <summary>
/// Elasticsearch implementation of document repository
/// MODULE 1: You'll implement the basic save/delete/get methods
/// MODULE 2: You'll implement the semantic search method
/// </summary>
public class ElasticsearchDocumentRepository : IDocumentRepository
{
    private readonly ElasticsearchClient _client;
    private readonly string _indexName;

    public ElasticsearchDocumentRepository(
        ElasticsearchClient client,
        IOptions<ElasticsearchSettings> options)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _indexName = options?.Value?.DefaultIndex ?? "rag-documents";
    }

    /// <summary>
    /// TODO - MODULE 1: Save document chunks to Elasticsearch
    /// Iterate through document.Chunks and index each one using _client.IndexAsync()
    /// </summary>
    public async Task<bool> SaveDocumentChunksAsync(Document document)
    {
        // TODO: Implement this method in Module 1
        throw new NotImplementedException("SaveDocumentChunksAsync - to be implemented in Module 1");
    }

    /// <summary>
    /// TODO - MODULE 2: Perform semantic search using kNN
    /// Use _client.SearchAsync() with KnnQuery to find similar chunks
    /// </summary>
    public async Task<List<SearchResult>> SearchAsync(float[] queryEmbedding, int topK = 5, float minScore = 0.7f)
    {
        // TODO: Implement this method in Module 2
        throw new NotImplementedException("SearchAsync - to be implemented in Module 2");
    }
}
