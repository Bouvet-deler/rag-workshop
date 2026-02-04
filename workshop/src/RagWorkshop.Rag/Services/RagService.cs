using Azure.AI.OpenAI;
using RagWorkshop.Rag.Interfaces;
using RagWorkshop.Repository.Interfaces;

namespace RagWorkshop.Rag.Services;

/// <summary>
/// RAG service implementation for retrieval and generation
/// MODULE 2: You'll implement semantic search and answer generation
/// </summary>
public class RagService : IRagService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly OpenAIClient? _openAIClient;
    private readonly string _embeddingDeploymentName;
    private readonly string _chatDeploymentName;

    public RagService(
        IDocumentRepository documentRepository,
        OpenAIClient? openAIClient = null,
        string embeddingDeploymentName = "text-embedding-3-small",
        string chatDeploymentName = "gpt-4o-mini")
    {
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        _openAIClient = openAIClient;
        _embeddingDeploymentName = embeddingDeploymentName;
        _chatDeploymentName = chatDeploymentName;
    }

    /// <summary>
    /// TODO - MODULE 2: Perform semantic search
    /// Steps:
    /// 1. Generate embedding for the query using Azure OpenAI
    /// 2. Search using the repository's SearchAsync method
    /// 3. Convert results to RAG SearchResult format
    /// </summary>
    public async Task<List<SearchResult>> SearchAsync(string query, int topK = 5, float minScore = 0.7f)
    {
        // TODO: Implement this method in Module 2
        throw new NotImplementedException("SearchAsync - to be implemented in Module 2");
    }

    /// <summary>
    /// TODO - MODULE 2: Generate answer using RAG
    /// Steps:
    /// 1. Perform semantic search to retrieve relevant chunks (RETRIEVAL)
    /// 2. Build context from retrieved chunks (AUGMENTATION)
    /// 3. Generate answer using Azure OpenAI chat (GENERATION)
    /// </summary>
    public async Task<RagResponse> GenerateAnswerAsync(string question, int topK = 5)
    {
        // TODO: Implement this method in Module 2
        throw new NotImplementedException("GenerateAnswerAsync - to be implemented in Module 2");
    }
}
