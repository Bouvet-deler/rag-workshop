namespace RagWorkshop.Rag.Interfaces;

/// <summary>
/// RAG service interface
/// </summary>
public interface IRagService
{
    Task<List<SearchResult>> SearchAsync(string query, int topK = 5, float minScore = 0.7f);
    Task<RagResponse> GenerateAnswerAsync(string question, int topK = 5);
}

/// <summary>
/// Search result from repository
/// </summary>
public class SearchResult
{
    public Chunk Chunk { get; set; } = new();
    public float Score { get; set; }
}

/// <summary>
/// Simplified chunk for RAG responses
/// </summary>
public class Chunk
{
    public string DocumentId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int PageNumber { get; set; }
}

/// <summary>
/// RAG response with answer and sources
/// </summary>
public class RagResponse
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public List<SourceChunk> Sources { get; set; } = new();
    public int TokensUsed { get; set; }
}

/// <summary>
/// Source chunk information
/// </summary>
public class SourceChunk
{
    public string DocumentId { get; set; } = string.Empty;
    public int PageNumber { get; set; }
    public string Text { get; set; } = string.Empty;
    public float Score { get; set; }
}
