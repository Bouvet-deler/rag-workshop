namespace RagWorkshop.Repository.Models;

/// <summary>
/// Represents a chunk of a document with its embedding
/// </summary>
public class DocumentChunk
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string DocumentId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int ChunkIndex { get; set; }
    public int PageNumber { get; set; }
    public float[]? Embedding { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
