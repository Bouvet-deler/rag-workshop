# Module 2: RAG - Search & Generation 🤖

**Duration:** ~45 minutes

In this module, you'll implement the RAG (Retrieval-Augmented Generation) pattern - semantic search using vector embeddings and AI-powered answer generation using retrieved context.

## 🎯 Learning Objectives

By the end of this module, you will:
- Implement semantic search using vector similarity (kNN)
- Build the complete RAG pattern: Retrieval + Augmentation + Generation
- Use Azure OpenAI to generate context-aware answers
- Create REST API endpoints for search and chat

---

## 🧠 Understanding RAG

**RAG** (Retrieval-Augmented Generation) is a pattern that combines:

1. **Retrieval**: Find relevant information from your documents using semantic search
2. **Augmentation**: Add that information as context to your prompt
3. **Generation**: Use an LLM to generate an answer based on the context

```
User Question
     ↓
[RETRIEVAL]
  Generate query embedding
  Search vector database
  Retrieve top-K similar chunks
     ↓
[AUGMENTATION]
  Build context from chunks
  Create prompt with context + question
     ↓
[GENERATION]
  Send to GPT-4o-mini
  Get contextual answer
     ↓
Return answer + sources
```

### Why RAG?

Without RAG:
- ❌ LLM only knows what it was trained on
- ❌ Cannot answer questions about your specific documents
- ❌ May hallucinate or give outdated information

With RAG:
- ✅ LLM has access to your specific documents
- ✅ Answers are grounded in your data
- ✅ Can cite sources for transparency
- ✅ Always up-to-date with your latest documents

---

## Part 1: Implement Vector Search

First, let's implement the semantic search functionality in the repository.

### 📝 Edit `src/RagWorkshop.Repository/Services/ElasticsearchDocumentRepository.cs`

Find the `SearchAsync` method (around line 90) and replace it:

```csharp
public async Task<List<SearchResult>> SearchAsync(float[] queryEmbedding, int topK = 5, float minScore = 0.7f)
{
    try
    {
        // Perform vector similarity search using kNN
        var searchResponse = await _client.SearchAsync<DocumentChunk>(s => s
            .Index(_indexName)
            .Size(topK)
            .Knn(knn => knn
                .Field(f => f.Embedding)
                .QueryVector(queryEmbedding)
                .k(topK)
                .NumCandidates(topK * 10) // Search a larger candidate pool for better results
            )
            .MinScore(minScore) // Only return results above the similarity threshold
        );

        if (!searchResponse.IsValidResponse || !searchResponse.Documents.Any())
            return new List<SearchResult>();

        // Map results to SearchResult objects with scores
        var results = new List<SearchResult>();
        foreach (var hit in searchResponse.Hits)
        {
            if (hit.Score.HasValue)
            {
                results.Add(new SearchResult
                {
                    Chunk = hit.Source!,
                    Score = (float)hit.Score.Value
                });
            }
        }

        return results;
    }
    catch
    {
        return new List<SearchResult>();
    }
}
```

### 💡 **What's Happening?**
- **kNN (k-Nearest Neighbors)**: Finds chunks with embeddings most similar to the query
- **QueryVector**: The embedding of the user's question
- **NumCandidates**: Searches a larger pool to find the best matches
- **MinScore**: Filters out low-similarity results (below 0.7)
- **Cosine similarity**: Elasticsearch compares vectors using cosine similarity

---

## Part 2: Fix RAG Interface

We need to update the RAG interface to match the repository's SearchResult type.

### 📝 Edit `src/RagWorkshop.Rag/Interfaces/IRagService.cs`

Replace the entire file to use the correct types:

```csharp
using RagWorkshop.Repository.Interfaces;

namespace RagWorkshop.Rag.Interfaces;

/// <summary>
/// Interface for RAG (Retrieval-Augmented Generation) service
/// </summary>
public interface IRagService
{
    Task<List<SearchResult>> SearchAsync(string query, int topK = 5, float minScore = 0.7f);
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
```

---

## Part 3: Implement RAG Service

Now let's implement the complete RAG logic.

### 📝 Edit `src/RagWorkshop.Rag/Services/RagService.cs`

Replace the entire file:

```csharp
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

    /// <summary>
    /// Semantic search: converts query to embedding and searches vector database
    /// </summary>
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

    /// <summary>
    /// Complete RAG: Retrieval + Augmentation + Generation
    /// </summary>
    public async Task<RagResponse> GenerateAnswerAsync(string question, int topK = 5)
    {
        if (_openAIClient == null)
        {
            throw new InvalidOperationException("OpenAI client not configured");
        }

        // ═══════════════════════════════════════════════════════
        // STEP 1: RETRIEVAL - Search for relevant chunks
        // ═══════════════════════════════════════════════════════
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

        // ═══════════════════════════════════════════════════════
        // STEP 2: AUGMENTATION - Build context from retrieved chunks
        // ═══════════════════════════════════════════════════════
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

        // ═══════════════════════════════════════════════════════
        // STEP 3: GENERATION - Call Azure OpenAI for answer
        // ═══════════════════════════════════════════════════════
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
```

### 💡 **What's Happening?**

**SearchAsync** (Retrieval):
1. Converts user query to embedding vector
2. Searches Elasticsearch using vector similarity
3. Returns top-K most similar chunks

**GenerateAnswerAsync** (Full RAG):
1. **RETRIEVAL**: Uses SearchAsync to find relevant chunks
2. **AUGMENTATION**: Builds context string from chunks + creates prompts
3. **GENERATION**: Sends to GPT-4o-mini for answer generation
4. Returns answer with source citations

---

## Part 4: Wire Up RAG Services

### 📝 Edit `src/RagWorkshop.Api/Extensions/RagServiceExtensions.cs`

Replace the entire file:

```csharp
using Azure.AI.OpenAI;
using RagWorkshop.Rag.Interfaces;
using RagWorkshop.Rag.Services;
using RagWorkshop.Repository.Interfaces;

namespace RagWorkshop.Api.Extensions;

public static class RagServiceExtensions
{
    public static IServiceCollection AddRagServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IRagService>(sp =>
        {
            var openAIClient = sp.GetService<OpenAIClient>();
            var documentRepository = sp.GetRequiredService<IDocumentRepository>();
            var config = sp.GetRequiredService<IConfiguration>();
            var embeddingDeploymentName = config["AzureOpenAI:EmbeddingDeploymentName"] ?? "text-embedding-3-small";
            var chatDeploymentName = config["AzureOpenAI:DeploymentName"] ?? "gpt-4o-mini";

            return new RagService(documentRepository, openAIClient, embeddingDeploymentName, chatDeploymentName);
        });

        return services;
    }
}
```

### 📝 Update `Program.cs`

Add RAG services. Update [Program.cs](../src/RagWorkshop.Api/Program.cs):

```csharp
builder.Services.AddElasticsearch(builder.Configuration);
builder.Services.AddAzureOpenAI(builder.Configuration);
builder.Services.AddIngestionServices(builder.Configuration);
builder.Services.AddRagServices(builder.Configuration);  // Add this line
```

---

## Part 5: RAG Controller

Finally, let's create the API endpoints for search and chat.

### 📝 Edit `src/RagWorkshop.Api/Controllers/RagController.cs`

Replace the entire file:

```csharp
using Microsoft.AspNetCore.Mvc;
using RagWorkshop.Rag.Interfaces;

namespace RagWorkshop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RagController : ControllerBase
{
    private readonly ILogger<RagController> _logger;
    private readonly IRagService _ragService;

    public RagController(ILogger<RagController> logger, IRagService ragService)
    {
        _logger = logger;
        _ragService = ragService;
    }

    /// <summary>
    /// Search for relevant document chunks using semantic similarity
    /// </summary>
    [HttpPost("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Search([FromBody] SearchRequest request)
    {
        _logger.LogInformation("Search endpoint called with query: {Query}", request.Query);

        try
        {
            var results = await _ragService.SearchAsync(
                request.Query, 
                request.TopK ?? 5, 
                request.MinScore ?? 0.7f);

            return Ok(new
            {
                query = request.Query,
                resultsCount = results.Count,
                results = results.Select(r => new
                {
                    text = r.Chunk.Text,
                    score = r.Score,
                    documentId = r.Chunk.DocumentId,
                    chunkIndex = r.Chunk.ChunkIndex,
                    pageNumber = r.Chunk.PageNumber
                })
            });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(503, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing semantic search");
            return StatusCode(500, new { error = "Search failed", details = ex.Message });
        }
    }

    /// <summary>
    /// Chat endpoint with RAG - retrieves context and generates answer
    /// </summary>
    [HttpPost("chat")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        _logger.LogInformation("Chat endpoint called with question: {Question}", request.Question);

        try
        {
            var response = await _ragService.GenerateAnswerAsync(request.Question, request.TopK ?? 5);

            return Ok(new
            {
                question = response.Question,
                answer = response.Answer,
                sources = response.Sources.Select(s => new
                {
                    text = s.Text.Length > 200 ? s.Text[..200] + "..." : s.Text,
                    score = s.Score,
                    documentId = s.DocumentId,
                    pageNumber = s.PageNumber,
                    chunkIndex = s.ChunkIndex
                }),
                tokensUsed = response.TokensUsed
            });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(503, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating RAG response");
            return StatusCode(500, new { error = "Chat failed", details = ex.Message });
        }
    }
}

public record ChatRequest(string Question, int? TopK = 5);
public record SearchRequest(string Query, int? TopK = 5, float? MinScore = 0.7f);
```

---

## 🧪 Testing the RAG System

### 1. Build and Run

```bash
cd workshop/src/RagWorkshop.Api
dotnet build
dotnet run
```

### 2. Upload a Document (if you haven't already)

```bash
curl -X POST http://localhost:5001/api/ingestion/upload \
  -H "Content-Type: application/json" \
  -d '{"filePath": "/path/to/your/document.pdf"}'
```

### 3. Test Semantic Search

```bash
curl -X POST http://localhost:5001/api/rag/search \
  -H "Content-Type: application/json" \
  -d '{"query": "What is machine learning?", "topK": 3}'
```

You should see:
```json
{
  "query": "What is machine learning?",
  "resultsCount": 3,
  "results": [
    {
      "text": "Machine learning is...",
      "score": 0.89,
      "documentId": "abc123",
      "pageNumber": 5,
      "chunkIndex": 12
    },
    ...
  ]
}
```

### 4. Test RAG Chat

```bash
curl -X POST http://localhost:5001/api/rag/chat \
  -H "Content-Type: application/json" \
  -d '{"question": "Explain machine learning in simple terms"}'
```

You should see:
```json
{
  "question": "Explain machine learning in simple terms",
  "answer": "Based on [Source 1], machine learning is a method...",
  "sources": [
    {
      "text": "Machine learning is...",
      "score": 0.89,
      "pageNumber": 5,
      ...
    }
  ],
  "tokensUsed": 450
}
```

### 5. Test in Swagger UI

Open [http://localhost:5001](http://localhost:5001) and try the `/api/rag/chat` endpoint interactively!

---

## 🎉 Module 2 Complete!

You've successfully implemented a complete RAG system that:
- ✅ Performs semantic search using vector embeddings
- ✅ Retrieves relevant context from documents
- ✅ Generates accurate, context-aware answers
- ✅ Cites sources for transparency
- ✅ Provides REST API endpoints for search and chat

### 📊 What We Created

| Component | Purpose |
|-----------|---------|
| **Vector Search** | kNN search in Elasticsearch |
| **RagService.SearchAsync** | Semantic search implementation |
| **RagService.GenerateAnswerAsync** | Complete RAG pipeline |
| **RagController** | HTTP endpoints for search & chat |
| **RagServiceExtensions** | DI configuration |

---

## 🧠 Understanding the Results

### Similarity Scores

- **0.9 - 1.0**: Excellent match (almost identical meaning)
- **0.7 - 0.9**: Good match (semantically similar)
- **0.5 - 0.7**: Moderate match (somewhat related)
- **< 0.5**: Poor match (probably not relevant)

We use `minScore: 0.7` to filter out irrelevant results.

### Temperature Parameter

In the chat completion:
- **0.0 - 0.3**: Deterministic, focused (good for facts)
- **0.7**: Balanced creativity and accuracy (what we use)
- **1.0 - 2.0**: Creative, varied (good for creative writing)

---

## 🚀 Enhancements You Could Add

Want to take this further? Try:

1. **Conversation History**: Add multi-turn conversations
2. **Hybrid Search**: Combine vector search with keyword search
3. **Re-ranking**: Re-rank results for better relevance
4. **Streaming Responses**: Stream GPT answers token-by-token
5. **Metadata Filtering**: Filter by document type, date, author
6. **Better Chunking**: Use semantic chunking or sentence-based chunking

---

## 🎓 What You've Learned

- **Vector similarity search** using Elasticsearch kNN
- **The RAG pattern**: Retrieval + Augmentation + Generation
- **Prompt engineering** for context-aware answers
- **Source citation** for transparency and verification
- **Building production-ready AI applications** with .NET and Azure

---

## 🏆 Congratulations!

You've completed the RAG Workshop! You now have a fully functional RAG system that can:
- Ingest PDF documents
- Generate and store vector embeddings
- Perform semantic search
- Generate AI-powered answers grounded in your documents

### 📚 Further Reading

- [RAG Patterns and Best Practices](https://learn.microsoft.com/azure/search/retrieval-augmented-generation-overview)
- [Azure OpenAI Documentation](https://learn.microsoft.com/azure/ai-services/openai/)
- [Elasticsearch Vector Search](https://www.elastic.co/guide/en/elasticsearch/reference/current/knn-search.html)
- [Prompt Engineering Guide](https://platform.openai.com/docs/guides/prompt-engineering)

---

## 🔍 Troubleshooting

**"OpenAI client not configured"**
- Check your Azure OpenAI credentials in appsettings.json or user secrets
- Verify your deployment names match

**Low similarity scores / No results**
- Try lowering `minScore` to 0.5 or 0.6
- Check that documents were indexed correctly
- Verify embeddings were generated (check Elasticvue)

**Empty or irrelevant answers**
- Check if relevant documents are indexed
- Try different search queries
- Increase `topK` to retrieve more context

---

**Thank you for completing the workshop! 🎉**

Want to see the complete solution? Check the `/solution` folder!
