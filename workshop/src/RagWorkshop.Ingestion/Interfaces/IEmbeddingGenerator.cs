using RagWorkshop.Models;

namespace RagWorkshop.Ingestion.Interfaces;

/// <summary>
/// Interface for generating embeddings
/// </summary>
public interface IEmbeddingGenerator
{
    /// <summary>
    /// Generate embeddings for a list of document chunks
    /// </summary>
    Task GenerateEmbeddingsAsync(List<DocumentChunk> chunks);
}
