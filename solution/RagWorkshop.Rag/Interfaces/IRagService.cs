using RagWorkshop.Repository.Interfaces;

namespace RagWorkshop.Rag.Interfaces;

/// <summary>
/// Interface for RAG (Retrieval-Augmented Generation) service
/// </summary>
public interface IRagService
{
    /// <summary>
    /// Search for relevant document chunks
    /// </summary>
    /// <param name="query">The search query</param>
    /// <param name="topK">Number of results to return</param>
    /// <param name="minScore">Minimum similarity score</param>
    /// <returns>List of search results</returns>
    Task<List<SearchResult>> SearchAsync(string query, int topK = 5, float minScore = 0.7f);

    /// <summary>
    /// Generate an answer using RAG - retrieves relevant context and generates response
    /// </summary>
    /// <param name="question">The user's question</param>
    /// <param name="topK">Number of document chunks to retrieve</param>
    /// <returns>RAG response with answer and sources</returns>
    Task<RagResponse> GenerateAnswerAsync(string question, int topK = 5);
}

/// <summary>
/// Response from RAG service containing answer and sources
/// </summary>
public class RagResponse
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public List<SourceChunk> Sources { get; set; } = new();
    public int TokensUsed { get; set; }
}

/// <summary>
/// Source chunk used in generating the answer
/// </summary>
public class SourceChunk
{
    public string Text { get; set; } = string.Empty;
    public float Score { get; set; }
    public string DocumentId { get; set; } = string.Empty;
    public int PageNumber { get; set; }
    public int ChunkIndex { get; set; }
}
