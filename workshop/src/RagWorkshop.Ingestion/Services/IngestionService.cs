using RagWorkshop.Ingestion.Interfaces;
using RagWorkshop.Models;
using RagWorkshop.Repository.Interfaces;

namespace RagWorkshop.Ingestion.Services;

/// <summary>
/// Orchestrates the document ingestion pipeline
/// MODULE 1: You'll implement this to coordinate the ingestion process
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

    /// <summary>
    /// TODO - MODULE 1: Process a PDF document through the entire pipeline
    /// Steps:
    /// 1. Extract text from PDF
    /// 2. Chunk the text
    /// 3. Generate embeddings
    /// 4. Save to repository
    /// </summary>
    public async Task<Document> ProcessDocumentAsync(Stream pdfStream, string fileName)
    {
        // TODO: Implement this method in Module 1
        throw new NotImplementedException("ProcessDocumentAsync - to be implemented in Module 1");
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
