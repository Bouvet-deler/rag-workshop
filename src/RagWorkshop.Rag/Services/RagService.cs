using Azure.AI.OpenAI;
using RagWorkshop.Rag.Interfaces;
using RagWorkshop.Repository.Interfaces;

namespace RagWorkshop.Rag.Services;

/// <summary>
/// RAG service implementation for retrieval and generation
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

    public async Task<List<SearchResult>> SearchAsync(string query, int topK = 5, float minScore = 0.7f)
    {
        if (_openAIClient == null)
        {
            throw new InvalidOperationException("OpenAI client not configured");
        }

        // Generate embedding for the query
        var embeddingResponse = await _openAIClient.GetEmbeddingsAsync(
            new EmbeddingsOptions(_embeddingDeploymentName, new[] { query })
        );

        var queryEmbedding = embeddingResponse.Value.Data[0].Embedding.ToArray();

        // Search using the repository
        return await _documentRepository.SearchAsync(queryEmbedding, topK, minScore);
    }

    public async Task<RagResponse> GenerateAnswerAsync(string question, int topK = 5)
    {
        if (_openAIClient == null)
        {
            throw new InvalidOperationException("OpenAI client not configured");
        }

        // Step 1: RETRIEVAL - Search for relevant chunks
        var searchResults = await SearchAsync(question, topK, minScore: 0.7f);

        if (!searchResults.Any())
        {
            return new RagResponse
            {
                Question = question,
                Answer = "I couldn't find any relevant information in the documents to answer your question.",
                Sources = new List<SourceChunk>(),
                TokensUsed = 0
            };
        }

        // Step 2: AUGMENTATION - Build context from retrieved chunks
        var context = string.Join("\n\n", searchResults.Select((r, i) =>
            $"[Source {i + 1}] (Page {r.Chunk.PageNumber}, Score: {r.Score:F2})\n{r.Chunk.Text}"));

        var systemPrompt = @"You are a helpful assistant that answers questions based on the provided context. 
        Use only the information from the context to answer the question. 
        If the context doesn't contain enough information to answer the question, say so.
        Always cite which source(s) you used by referencing [Source N] in your answer.";

        var userPrompt = $@"Context:
        {context}

        Question: {question}

        Answer:";

        // Step 3: GENERATION - Call Azure OpenAI
        var chatOptions = new ChatCompletionsOptions
        {
            DeploymentName = _chatDeploymentName,
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage(userPrompt)
            },
            Temperature = 0.7f,
            MaxTokens = 800
        };

        var chatResponse = await _openAIClient.GetChatCompletionsAsync(chatOptions);
        var answer = chatResponse.Value.Choices[0].Message.Content;
        var tokensUsed = chatResponse.Value.Usage.TotalTokens;

        // Build response with sources
        var sources = searchResults.Select(r => new SourceChunk
        {
            Text = r.Chunk.Text,
            Score = r.Score,
            DocumentId = r.Chunk.DocumentId,
            PageNumber = r.Chunk.PageNumber,
            ChunkIndex = r.Chunk.ChunkIndex
        }).ToList();

        return new RagResponse
        {
            Question = question,
            Answer = answer,
            Sources = sources,
            TokensUsed = tokensUsed
        };
    }
}
