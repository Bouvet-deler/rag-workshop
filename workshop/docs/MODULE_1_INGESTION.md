# Module 1: Document Ingestion Pipeline 📥

**Duration:** ~60 minutes

In this module, you'll build the complete document ingestion pipeline that processes PDF files, extracts text, chunks it intelligently, generates embeddings using Azure OpenAI, and stores everything in Elasticsearch for semantic search.

## 🎯 Learning Objectives

By the end of this module, you will:
- Extract text from PDF files using iText7
- Implement text chunking with overlap
- Generate vector embeddings using Azure OpenAI
- Store document chunks with embeddings in Elasticsearch
- Create proper Elasticsearch index mappings for vector search
- Wire up the complete ingestion pipeline

---

## 📊 Ingestion Pipeline Overview

```
PDF File (Stream)
    ↓
┌──────────────────┐
│  PDF Extractor   │  Extract text + page numbers
└────────┬─────────┘
         ↓
     List<PageContent>
         ↓
┌──────────────────┐
│  Text Chunker    │  Split into chunks (500 char, 50 overlap)
└────────┬─────────┘
         ↓
     List<DocumentChunk>
         ↓
┌──────────────────┐
│  Embedding Gen   │  Generate 1536-dim vectors
└────────┬─────────┘
         ↓
     DocumentChunk[] with Embeddings
         ↓
┌──────────────────┐
│  Repository      │  Save to Elasticsearch
└──────────────────┘
```

---

## Part 1: PDF Text Extraction

Let's start by implementing the PDF extractor that reads PDF files and extracts text with page information.

### 📝 Edit `src/RagWorkshop.Ingestion/Services/PdfExtractor.cs`

Replace the entire file:

```csharp
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using RagWorkshop.Ingestion.Interfaces;

namespace RagWorkshop.Ingestion.Services;

/// <summary>
/// PDF text extraction using iText7
/// </summary>
public class PdfExtractor : IPdfExtractor
{
    public async Task<string> ExtractTextAsync(Stream pdfStream)
    {
        return await Task.Run(() =>
        {
            using var pdfReader = new PdfReader(pdfStream);
            using var pdfDocument = new PdfDocument(pdfReader);

            var text = string.Empty;
            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
            {
                var page = pdfDocument.GetPage(i);
                var strategy = new SimpleTextExtractionStrategy();
                text += PdfTextExtractor.GetTextFromPage(page, strategy);
            }

            return text;
        });
    }

    public async Task<List<PageContent>> ExtractTextWithPagesAsync(Stream pdfStream)
    {
        return await Task.Run(() =>
        {
            using var pdfReader = new PdfReader(pdfStream);
            using var pdfDocument = new PdfDocument(pdfReader);

            var pages = new List<PageContent>();
            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
            {
                var page = pdfDocument.GetPage(i);
                var strategy = new SimpleTextExtractionStrategy();
                var text = PdfTextExtractor.GetTextFromPage(page, strategy);

                pages.Add(new PageContent
                {
                    PageNumber = i,
                    Text = text
                });
            }

            return pages;
        });
    }
}
```

### 💡 **What's Happening?**
- Uses iText7 library to read PDF files
- `PdfReader` opens the PDF stream
- `PdfDocument` provides access to pages
- `SimpleTextExtractionStrategy` extracts text content
- Returns a list of `PageContent` with page numbers for better citation

---

## Part 2: Text Chunking

Now let's implement the text chunker that splits long text into manageable pieces with overlap.

### 📝 Edit `src/RagWorkshop.Ingestion/Services/SimpleTextChunker.cs`

Replace the entire file:

```csharp
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
```

### 💡 **What's Happening?**
- **Chunk size**: 500 characters (configurable)
- **Overlap**: 50 characters between chunks
- Overlap ensures context isn't lost at chunk boundaries
- Each chunk gets metadata (start/end indices, page number)
- Chunks are created with a sliding window approach

### Why Overlap?
Without overlap: `"...end of chunk 1"|"start of chunk 2..."`  
With overlap: `"...end of chunk 1 overlap"|"overlap start of chunk 2..."`

This prevents splitting important context across chunk boundaries!

---

## Part 3: Embedding Generation

Let's generate vector embeddings using Azure OpenAI.

### 📝 Edit `src/RagWorkshop.Ingestion/Services/AzureOpenAIEmbeddingGenerator.cs`

Replace the entire file:

```csharp
using Azure.AI.OpenAI;
using RagWorkshop.Ingestion.Interfaces;
using RagWorkshop.Repository.Models;

namespace RagWorkshop.Ingestion.Services;

/// <summary>
/// Generate embeddings using Azure OpenAI
/// </summary>
public class AzureOpenAIEmbeddingGenerator : IEmbeddingGenerator
{
    private readonly OpenAIClient _client;
    private readonly string _deploymentName;

    public AzureOpenAIEmbeddingGenerator(OpenAIClient client, string deploymentName = "text-embedding-3-small")
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _deploymentName = deploymentName;
    }

    public async Task GenerateEmbeddingsAsync(List<DocumentChunk> chunks)
    {
        if (!chunks.Any())
            return;

        // Extract texts from chunks
        var texts = chunks.Select(c => c.Text).ToList();

        // Generate embeddings for all texts in one API call (batch processing)
        var embeddingsOptions = new EmbeddingsOptions(_deploymentName, texts);
        var response = await _client.GetEmbeddingsAsync(embeddingsOptions);

        // Assign embeddings back to chunks
        for (int i = 0; i < chunks.Count; i++)
        {
            chunks[i].Embedding = response.Value.Data[i].Embedding.ToArray();
        }
    }
}
```

### 💡 **What's Happening?**
- Uses Azure OpenAI's embedding API
- Sends multiple texts in a single batch (more efficient)
- text-embedding-3-small model generates 1536-dimensional vectors
- Each vector represents the semantic meaning of the text
- Similar texts will have vectors close together in vector space

---

## Part 4: Elasticsearch Index Initialization

Before we can save documents, we need to create an Elasticsearch index with the proper mapping for vector search.

### 📝 Edit `src/RagWorkshop.Api/Services/ElasticsearchInitializer.cs`

Replace the entire file:

```csharp
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;
using RagWorkshop.Repository.Models;

namespace RagWorkshop.Api.Services;

/// <summary>
/// Service responsible for initializing Elasticsearch index with proper mappings
/// </summary>
public class ElasticsearchInitializer
{
    private readonly ElasticsearchClient _client;
    private readonly ILogger<ElasticsearchInitializer> _logger;
    private readonly string _indexName;

    public ElasticsearchInitializer(
        ElasticsearchClient client,
        ILogger<ElasticsearchInitializer> logger,
        IConfiguration configuration)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _indexName = configuration["Elasticsearch:DefaultIndex"] ?? "rag-documents";
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Check if index exists
            var existsResponse = await _client.Indices.ExistsAsync(_indexName);

            if (existsResponse.Exists)
            {
                _logger.LogInformation("Elasticsearch index '{IndexName}' already exists", _indexName);
                return;
            }

            // Create index with mapping for vector search
            var createResponse = await _client.Indices.CreateAsync(_indexName, c => c
                .Mappings(m => m
                    .Properties<DocumentChunk>(p => p
                        .Keyword(k => k.Id)
                        .Keyword(k => k.DocumentId)
                        .Text(t => t.Text)
                        .IntegerNumber(i => i.ChunkIndex)
                        .IntegerNumber(i => i.PageNumber)
                        .DenseVector(d => d.Embedding, dv => dv
                            .Dims(1536)  // Azure OpenAI text-embedding-3-small dimension
                            .Similarity("cosine"))  // Cosine similarity for vector search
                    )
                )
            );

            if (createResponse.IsValidResponse)
            {
                _logger.LogInformation("Successfully created Elasticsearch index '{IndexName}' with vector mapping", _indexName);
            }
            else
            {
                _logger.LogError("Failed to create Elasticsearch index: {Error}",
                    createResponse.ElasticsearchServerError?.Error?.Reason);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing Elasticsearch index");
        }
    }
}
```

### 💡 **What's Happening?**
- Creates an Elasticsearch index with proper field mappings
- **DenseVector** field type for embeddings (1536 dimensions)
- **Cosine similarity** for comparing vectors
- Only creates the index if it doesn't already exist
- Logs success/failure for debugging

---

## Part 5: Document Repository

Now let's implement the repository that saves and retrieves documents from Elasticsearch.

### 📝 Edit `src/RagWorkshop.Repository/Services/ElasticsearchDocumentRepository.cs`

Replace the entire file:

```csharp
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Options;
using RagWorkshop.Repository.Models;
using RagWorkshop.Repository.Interfaces;
using RagWorkshop.Repository.Settings;

namespace RagWorkshop.Repository.Services;

/// <summary>
/// Elasticsearch implementation of document repository
/// </summary>
public class ElasticsearchDocumentRepository : IDocumentRepository
{
    private readonly ElasticsearchClient _client;
    private readonly string _indexName;

    public ElasticsearchDocumentRepository(
        ElasticsearchClient client,
        IOptions<ElasticsearchSettings> options)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _indexName = options?.Value?.DefaultIndex ?? "rag-documents";
    }

    public async Task<bool> SaveDocumentChunksAsync(Document document)
    {
        try
        {
            // Index each chunk
            foreach (var chunk in document.Chunks)
            {
                var chunkResponse = await _client.IndexAsync(chunk, idx => idx
                    .Index(_indexName)
                    .Id(chunk.Id));

                if (!chunkResponse.IsValidResponse)
                {
                    return false;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteDocumentAsync(string documentId)
    {
        try
        {
            // Delete all chunks for this document
            var deleteByQueryResponse = await _client.DeleteByQueryAsync<DocumentChunk>(_indexName, d => d
                .Query(q => q
                    .Term(t => t.Field(f => f.DocumentId).Value(documentId))
                )
            );

            return deleteByQueryResponse.IsValidResponse;
        }
        catch
        {
            return false;
        }
    }

    public async Task<Document?> GetDocumentAsync(string documentId)
    {
        try
        {
            // Get all chunks for this document
            var searchResponse = await _client.SearchAsync<DocumentChunk>(s => s
                .Index(_indexName)
                .Query(q => q
                    .Term(t => t.Field(f => f.DocumentId).Value(documentId))
                )
            );

            if (!searchResponse.IsValidResponse || !searchResponse.Documents.Any())
                return null;

            var chunks = searchResponse.Documents.ToList();

            return new Document
            {
                Id = documentId,
                Chunks = chunks
            };
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<SearchResult>> SearchAsync(float[] queryEmbedding, int topK = 5, float minScore = 0.7f)
    {
        // We'll implement this in Module 2
        throw new NotImplementedException("SearchAsync - to be implemented in Module 2");
    }
}
```

### 💡 **What's Happening?**
- **SaveDocumentChunksAsync**: Indexes each chunk with its embedding
- **DeleteDocumentAsync**: Removes all chunks for a document
- **GetDocumentAsync**: Retrieves all chunks for a document
- SearchAsync is left for Module 2 (RAG functionality)

---

## Part 6: Ingestion Service Orchestrator

Now let's orchestrate the entire pipeline.

### 📝 Edit `src/RagWorkshop.Ingestion/Services/IngestionService.cs`

Replace the entire file:

```csharp
using RagWorkshop.Ingestion.Interfaces;
using RagWorkshop.Repository.Models;
using RagWorkshop.Repository.Interfaces;

namespace RagWorkshop.Ingestion.Services;

/// <summary>
/// Orchestrates the document ingestion pipeline
/// </summary>
public class IngestionService
{
    private readonly IPdfExtractor? _pdfExtractor;
    private readonly ITextChunker? _textChunker;
    private readonly IEmbeddingGenerator? _embeddingGenerator;
    private readonly IDocumentRepository? _documentRepository;

    public IngestionService(
        IPdfExtractor? pdfExtractor = null,
        ITextChunker? textChunker = null,
        IEmbeddingGenerator? embeddingGenerator = null,
        IDocumentRepository? documentRepository = null)
    {
        _pdfExtractor = pdfExtractor;
        _textChunker = textChunker;
        _embeddingGenerator = embeddingGenerator;
        _documentRepository = documentRepository;
    }

    public async Task<Document> ProcessDocumentAsync(Stream pdfStream, string fileName)
    {
        var document = CreateDocument(fileName, pdfStream.Length);

        try
        {
            // Step 1: Extract PDF text with page numbers
            var pages = await ExtractPdfTextAsync(pdfStream);

            // Step 2: Chunk text from all pages
            var chunks = CreateChunksFromPages(pages, document.Id);

            // Step 3: Generate embeddings for all chunks
            await GenerateEmbeddingsForChunksAsync(chunks);

            // Step 4: Save to Elasticsearch
            document.Chunks = chunks;
            await IndexDocumentAsync(document);

            return document;
        }
        catch (Exception ex)
        {
            document.Status = "failed";
            throw new InvalidOperationException($"Failed to process document: {ex.Message}", ex);
        }
    }

    private Document CreateDocument(string fileName, long fileSize)
    {
        return new Document
        {
            FileName = fileName,
            ContentType = "application/pdf",
            Status = "processing",
            FileSize = fileSize
        };
    }

    private async Task<List<PageContent>> ExtractPdfTextAsync(Stream pdfStream)
    {
        if (_pdfExtractor == null)
            throw new InvalidOperationException("PDF extractor not configured");

        return await _pdfExtractor.ExtractTextWithPagesAsync(pdfStream);
    }

    private List<DocumentChunk> CreateChunksFromPages(List<PageContent> pages, string documentId)
    {
        if (_textChunker == null)
            throw new InvalidOperationException("Text chunker not configured");

        var chunks = new List<DocumentChunk>();
        foreach (var page in pages)
        {
            var pageChunks = _textChunker.ChunkText(page.Text, documentId, page.PageNumber);
            chunks.AddRange(pageChunks);
        }

        return chunks;
    }

    private async Task GenerateEmbeddingsForChunksAsync(List<DocumentChunk> chunks)
    {
        if (_embeddingGenerator == null)
            return;

        await _embeddingGenerator.GenerateEmbeddingsAsync(chunks);
    }

    private async Task IndexDocumentAsync(Document document)
    {
        if (_documentRepository != null)
        {
            var indexed = await _documentRepository.SaveDocumentChunksAsync(document);
            document.Status = indexed ? "completed" : "failed";
        }
        else
        {
            document.Status = "completed_no_indexing";
        }
    }
}
```

---

## Part 7: Wire Up Services (Dependency Injection)

Now let's configure all services in the API.

### 📝 Edit `src/RagWorkshop.Api/Extensions/IngestionServiceExtensions.cs`

Replace the entire file:

```csharp
using Azure.AI.OpenAI;
using RagWorkshop.Ingestion.Interfaces;
using RagWorkshop.Ingestion.Services;
using RagWorkshop.Repository.Interfaces;
using RagWorkshop.Repository.Services;

namespace RagWorkshop.Api.Extensions;

public static class IngestionServiceExtensions
{
    public static IServiceCollection AddIngestionServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Repository
        services.AddScoped<IDocumentRepository, ElasticsearchDocumentRepository>();

        // Chunking
        services.AddScoped<ITextChunker, SimpleTextChunker>();

        // PDF Extraction
        services.AddScoped<IPdfExtractor, PdfExtractor>();

        // Embedding Generation (requires Azure OpenAI)
        services.AddScoped<IEmbeddingGenerator>(sp =>
        {
            var openAIClient = sp.GetService<OpenAIClient>();
            var config = sp.GetRequiredService<IConfiguration>();
            var deploymentName = config["AzureOpenAI:EmbeddingDeploymentName"] ?? "text-embedding-3-small";

            if (openAIClient == null)
            {
                throw new InvalidOperationException("Azure OpenAI client not configured. Set credentials in appsettings.json");
            }

            return new AzureOpenAIEmbeddingGenerator(openAIClient, deploymentName);
        });

        // Ingestion Service (orchestrator)
        services.AddScoped<IngestionService>();

        // Elasticsearch Initializer
        services.AddScoped<RagWorkshop.Api.Services.ElasticsearchInitializer>();

        return services;
    }
}
```

### 📝 Update `Program.cs`

Add the ingestion services and index initialization. Update [Program.cs](../src/RagWorkshop.Api/Program.cs):

```csharp
using RagWorkshop.Api.Extensions;
using RagWorkshop.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "RAG Workshop API", Version = "v1" });
});

// Configure services
builder.Services.AddElasticsearch(builder.Configuration);
builder.Services.AddAzureOpenAI(builder.Configuration);
builder.Services.AddIngestionServices(builder.Configuration);  // Add this line

var app = builder.Build();

// Initialize Elasticsearch index (Add this block)
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<ElasticsearchInitializer>();
    await initializer.InitializeAsync();
}

// Configure HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RAG Workshop API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
}));

app.Run();
```

---

## Part 8: Ingestion Controller

Finally, let's implement the API endpoint for uploading PDFs.

### 📝 Edit `src/RagWorkshop.Api/Controllers/IngestionController.cs`

Replace the entire file:

```csharp
using Microsoft.AspNetCore.Mvc;
using RagWorkshop.Ingestion.Services;
using RagWorkshop.Repository.Interfaces;

namespace RagWorkshop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IngestionController : ControllerBase
{
    private readonly ILogger<IngestionController> _logger;
    private readonly IngestionService _ingestionService;
    private readonly IDocumentRepository _documentRepository;

    public IngestionController(
        ILogger<IngestionController> logger,
        IngestionService ingestionService,
        IDocumentRepository documentRepository)
    {
        _logger = logger;
        _ingestionService = ingestionService;
        _documentRepository = documentRepository;
    }

    [HttpPost("upload")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadPdf([FromBody] UploadRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.FilePath))
            {
                return BadRequest(new { error = "FilePath is required" });
            }

            if (!System.IO.File.Exists(request.FilePath))
            {
                return BadRequest(new { error = $"PDF file not found at path: {request.FilePath}" });
            }

            _logger.LogInformation("Processing PDF from path: {FilePath}", request.FilePath);

            // Read PDF file
            using var fileStream = new FileStream(request.FilePath, FileMode.Open, FileAccess.Read);
            var fileName = Path.GetFileName(request.FilePath);

            // Process through ingestion pipeline
            var document = await _ingestionService.ProcessDocumentAsync(fileStream, fileName);

            _logger.LogInformation("Successfully processed document {DocumentId} with {ChunkCount} chunks",
                document.Id, document.Chunks.Count);

            return Ok(new
            {
                documentId = document.Id,
                fileName = document.FileName,
                status = document.Status,
                chunksCreated = document.Chunks.Count,
                message = "Document processed and indexed successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PDF upload");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

public class UploadRequest
{
    public string FilePath { get; set; } = string.Empty;
}
```

---

## 🧪 Testing the Ingestion Pipeline

### 1. Build and Run

```bash
cd workshop/src/RagWorkshop.Api
dotnet build
dotnet run
```

### 2. Test with a PDF

Create a simple test PDF or use any existing PDF file. Then:

```bash
curl -X POST http://localhost:5001/api/ingestion/upload \
  -H "Content-Type: application/json" \
  -d '{"filePath": "/path/to/your/document.pdf"}'
```

You should see a response like:

```json
{
  "documentId": "abc123...",
  "fileName": "document.pdf",
  "status": "completed",
  "chunksCreated": 25,
  "message": "Document processed and indexed successfully"
}
```

### 3. Verify in Elasticvue

Open [http://localhost:8080](http://localhost:8080) and:
1. Go to **Indices** tab
2. You should see `rag-documents` index
3. Click on it to see the document count
4. Go to **Search** tab and browse the chunks!

---

## 🎉 Module 1 Complete!

You've successfully built a complete document ingestion pipeline that:
- ✅ Extracts text from PDF files with page numbers
- ✅ Chunks text into manageable pieces with overlap
- ✅ Generates vector embeddings using Azure OpenAI
- ✅ Stores everything in Elasticsearch with proper vector mappings
- ✅ Provides an API endpoint for document upload

### 📊 What We Created

| Component | Purpose |
|-----------|---------|
| **PdfExtractor** | Extract text from PDFs |
| **SimpleTextChunker** | Split text into overlapping chunks |
| **AzureOpenAIEmbeddingGenerator** | Generate embeddings |
| **ElasticsearchDocumentRepository** | Save/retrieve from Elasticsearch |
| **IngestionService** | Orchestrate the pipeline |
| **IngestionController** | HTTP API endpoint |
| **ElasticsearchInitializer** | Create index with vector mapping |

---

## 🚀 Next Steps

**[Continue to Module 2: RAG - Search & Generation →](MODULE_2_RAG.md)**

In Module 2, you'll implement:
- Semantic search using vector similarity
- Context-aware answer generation with GPT-4o-mini
- The complete RAG (Retrieval-Augmented Generation) pattern!
