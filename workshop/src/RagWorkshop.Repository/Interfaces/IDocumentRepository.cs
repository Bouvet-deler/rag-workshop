using RagWorkshop.Repository.Models;

namespace RagWorkshop.Repository.Interfaces;

/// <summary>
/// Repository interface for document operations
/// </summary>
public interface IDocumentRepository
{
    Task<bool> SaveDocumentChunksAsync(Document document);
    Task<bool> DeleteDocumentAsync(string documentId);
    Task<Document?> GetDocumentAsync(string documentId);
    Task<List<SearchResult>> SearchAsync(float[] queryEmbedding, int topK = 5, float minScore = 0.7f);
}

/// <summary>
/// Search result with chunk and similarity score
/// </summary>
public class SearchResult
{
    public DocumentChunk Chunk { get; set; } = new();
    public float Score { get; set; }
}
