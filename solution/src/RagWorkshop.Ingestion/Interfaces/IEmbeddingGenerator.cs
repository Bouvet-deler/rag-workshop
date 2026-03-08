namespace RagWorkshop.Ingestion.Interfaces;

/// <summary>
/// Interface for generating embeddings
/// </summary>
public interface IEmbeddingGenerator
{
    /// <summary>
    /// Generate embeddings for multiple texts
    /// </summary>
    Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts);
}
