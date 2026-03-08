using RagWorkshop.Models;

namespace RagWorkshop.Repository.Interfaces;

/// <summary>
/// Interface for document repository - CRUD and search operations
/// </summary>
public interface IDocumentRepository
{
    /// <summary>
    /// Index/save document chunks
    /// </summary>
    Task<bool> SaveDocumentChunksAsync(Document document);

    /// <summary>
    /// Search for document chunks using semantic similarity
    /// </summary>
    /// <param name="queryEmbedding">The embedding vector of the search query</param>
    /// <param name="topK">Number of results to return (default: 5)</param>
    /// <param name="minScore">Minimum similarity score threshold (default: 0.7)</param>
    /// <returns>List of relevant document chunks with their similarity scores</returns>
    Task<List<SearchResult>> SearchAsync(float[] queryEmbedding, int topK = 5, float minScore = 0.7f);
}

/// <summary>
/// Represents a search result with the chunk and its relevance score
/// </summary>
public class SearchResult
{
    public DocumentChunk Chunk { get; set; } = new();
    public float Score { get; set; }
}
