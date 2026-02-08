using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Options;
using RagWorkshop.Ingestion.Interfaces;
using RagWorkshop.Models;
using RagWorkshop.Repository.Settings;

namespace RagWorkshop.Ingestion.Services;

/// <summary>
/// Index documents in Elasticsearch
/// </summary>
public class ElasticsearchDocumentIndexer : IDocumentIndexer
{
    private readonly ElasticsearchClient _client;
    private readonly string _indexName;

    public ElasticsearchDocumentIndexer(
        ElasticsearchClient client,
        IOptions<ElasticsearchSettings> options)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _indexName = options?.Value?.DefaultIndex ?? "rag-documents";
    }

    public async Task<bool> IndexDocumentAsync(Document document)
    {
        try
        {
            // Index each chunk with document metadata embedded
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
            // Retrieve all chunks for this document
            var searchResponse = await _client.SearchAsync<DocumentChunk>(s => s
                .Index(_indexName)
                .Query(q => q
                    .Term(t => t.Field(f => f.DocumentId).Value(documentId))
                )
                .Size(1000) // Assuming a document won't have more than 1000 chunks
            );

            if (!searchResponse.IsValidResponse || searchResponse.Hits.Count == 0)
            {
                return null;
            }

            var chunks = searchResponse.Hits.Select(h => h.Source).ToList();

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
}
