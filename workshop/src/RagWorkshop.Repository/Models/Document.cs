namespace RagWorkshop.Repository.Models;

/// <summary>
/// Represents a document with its chunks
/// </summary>
public class Document
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public long FileSize { get; set; }
    public string Status { get; set; } = "processing";
    public List<DocumentChunk> Chunks { get; set; } = new();
}
