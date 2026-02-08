using RagWorkshop.Ingestion.Interfaces;
using RagWorkshop.Models;
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

    /// <summary>
    /// Process a PDF document: extract text, chunk, generate embeddings, and index
    /// </summary>
    public async Task<Document> ProcessDocumentAsync(Stream pdfStream, string fileName)
    {
        var document = CreateDocument(fileName, pdfStream.Length);

        try
        {
            var pages = await ExtractPdfTextAsync(pdfStream);
            var chunks = CreateChunksFromPages(pages, document.Id);
            await GenerateEmbeddingsForChunksAsync(chunks);

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

        var chunkTexts = chunks.Select(c => c.Text).ToList();
        var embeddings = await _embeddingGenerator.GenerateEmbeddingsAsync(chunkTexts);

        for (int i = 0; i < chunks.Count; i++)
        {
            chunks[i].Embedding = embeddings[i];
        }
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
