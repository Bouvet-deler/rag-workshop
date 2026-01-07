using RagWorkshop.Ingestion.Interfaces;
using RagWorkshop.Repository.Models;

namespace RagWorkshop.Ingestion.Services;

/// <summary>
/// Simple text chunker with fixed size and overlap
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

    public List<DocumentChunk> ChunkText(string text, string documentId, int pageNumber = 0)
    {
        var chunks = new List<DocumentChunk>();

        if (string.IsNullOrWhiteSpace(text))
            return chunks;

        var startIndex = 0;
        var chunkIndex = 0;

        while (startIndex < text.Length)
        {
            var length = Math.Min(_chunkSize, text.Length - startIndex);
            var chunkText = text.Substring(startIndex, length);

            chunks.Add(new DocumentChunk
            {
                DocumentId = documentId,
                Text = chunkText,
                ChunkIndex = chunkIndex++,
                PageNumber = pageNumber,
                Metadata = new Dictionary<string, object>
                {
                    ["start_index"] = startIndex,
                    ["end_index"] = startIndex + length
                }
            });

            // Move to next chunk with overlap
            startIndex += _chunkSize - _overlap;
        }

        return chunks;
    }
}
