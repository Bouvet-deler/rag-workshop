namespace RagWorkshop.Ingestion.Interfaces;

/// <summary>
/// Interface for PDF text extraction
/// </summary>
public interface IPdfExtractor
{
    /// <summary>
    /// Extract text with page information
    /// </summary>
    Task<List<PageContent>> ExtractTextWithPagesAsync(Stream pdfStream);
}

/// <summary>
/// Represents text content from a PDF page
/// </summary>
public class PageContent
{
    public int PageNumber { get; set; }
    public string Text { get; set; } = string.Empty;
}
