using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Options;
using RagWorkshop.Repository.Models;
using RagWorkshop.Repository.Interfaces;
using RagWorkshop.Repository.Settings;

namespace RagWorkshop.Repository.Services;

/// <summary>
/// Elasticsearch implementation of document repository
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

    public async Task<bool> SaveDocumentChunksAsync(Document document)
    {
        try
        {
            // Index each chunk
            foreach (var chunk in document.Chunks)
            {
                var chunkResponse = await _client.IndexAsync(chunk, idx => idx
                    .Index(_indexName)
                    .Id(chunk.Id));

                if (!chunkResponse.IsValidResponse)
                {
                    return false;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteDocumentAsync(string documentId)
    {
        try
        {
            // Delete all chunks for this document
            var deleteByQueryResponse = await _client.DeleteByQueryAsync<DocumentChunk>(_indexName, d => d
                .Query(q => q
                    .Term(t => t.Field(f => f.DocumentId).Value(documentId))
                )
            );

            return deleteByQueryResponse.IsValidResponse;
        }
        catch
        {
            return false;
        }
    }

    public async Task<Document?> GetDocumentAsync(string documentId)
    {
        try
        {
            // Get all chunks for this document
            var searchResponse = await _client.SearchAsync<DocumentChunk>(s => s
                .Index(_indexName)
                .Query(q => q
                    .Term(t => t.Field(f => f.DocumentId).Value(documentId))
                )
            );

            if (!searchResponse.IsValidResponse || !searchResponse.Documents.Any())
                return null;

            var chunks = searchResponse.Documents.ToList();

            return new Document
            {
                Id = documentId,
                Chunks = chunks
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<Document>> GetAllDocumentsAsync()
    {
        try
        {
            // Get all chunks
            var searchResponse = await _client.SearchAsync<DocumentChunk>(s => s
                .Index(_indexName)
                .Size(1000)
            );

            if (!searchResponse.IsValidResponse || !searchResponse.Documents.Any())
                return new List<Document>();

            // Group chunks by document ID
            var documentGroups = searchResponse.Documents.GroupBy(c => c.DocumentId);

            return documentGroups.Select(g => new Document
            {
                Id = g.Key,
                Chunks = g.ToList()
            }).ToList();
        }
        catch
        {
            return new List<Document>();
        }
    }

    public async Task<List<SearchResult>> SearchAsync(float[] queryEmbedding, int topK = 5, float minScore = 0.7f)
    {
        try
        {
            // Perform vector similarity search using kNN
            var searchResponse = await _client.SearchAsync<DocumentChunk>(s => s
                .Index(_indexName)
                .Size(topK)
                .Knn(knn => knn
                    .Field(f => f.Embedding)
                    .QueryVector(queryEmbedding)
                    .k(topK)
                    .NumCandidates(topK * 10) // Search a larger candidate pool for better results
                )
                .MinScore(minScore) // Only return results above the similarity threshold
            );

            if (!searchResponse.IsValidResponse || !searchResponse.Documents.Any())
                return new List<SearchResult>();

            // Map results to SearchResult objects with scores
            var results = new List<SearchResult>();
            foreach (var hit in searchResponse.Hits)
            {
                if (hit.Score.HasValue)
                {
                    results.Add(new SearchResult
                    {
                        Chunk = hit.Source!,
                        Score = (float)hit.Score.Value
                    });
                }
            }

            return results;
        }
        catch
        {
            return new List<SearchResult>();
        }
    }
}
