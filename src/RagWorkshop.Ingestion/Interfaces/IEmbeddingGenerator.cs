namespace RagWorkshop.Ingestion.Interfaces;

/// <summary>
/// Interface for generating embeddings
/// </summary>
public interface IEmbeddingGenerator
{
    /// <summary>
    /// Generate embedding for a single text
    /// </summary>
    Task<float[]> GenerateEmbeddingAsync(string text);

    /// <summary>
    /// Generate embeddings for multiple texts
    /// </summary>
    Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts);
}
