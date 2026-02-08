using RagWorkshop.Models;

namespace RagWorkshop.Ingestion.Interfaces;

/// <summary>
/// Interface for text chunking strategies
/// </summary>
public interface ITextChunker
{
    /// <summary>
    /// Split text into chunks
    /// </summary>
    List<DocumentChunk> ChunkText(string text, string documentId, int pageNumber = 0);
}
