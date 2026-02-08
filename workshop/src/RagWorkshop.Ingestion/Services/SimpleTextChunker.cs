using RagWorkshop.Ingestion.Interfaces;
using RagWorkshop.Models;

namespace RagWorkshop.Ingestion.Services;

/// <summary>
/// Simple text chunker with fixed size and overlap
/// MODULE 1: You'll implement this to split text into chunks
/// </summary>
public class SimpleTextChunker : ITextChunker
{
    private readonly int _chunkSize;
    private readonly int _overlap;

    public SimpleTextChunker(int chunkSize = 500, int overlap = 50)
    {
        _chunkSize = chunkSize;
        _overlap = overlap;
    }

    /// <summary>
    /// TODO - MODULE 1: Chunk text with overlap
    /// Split text into chunks of size _chunkSize with _overlap between chunks
    /// Each chunk should be a DocumentChunk with proper metadata
    /// </summary>
    public List<DocumentChunk> ChunkText(string text, string documentId, int pageNumber = 0)
    {
        // TODO: Implement this method in Module 1
        throw new NotImplementedException("ChunkText - to be implemented in Module 1");
    }
}
