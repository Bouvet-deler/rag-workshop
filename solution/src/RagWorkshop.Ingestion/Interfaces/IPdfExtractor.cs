namespace RagWorkshop.Ingestion.Interfaces;

/// <summary>
/// Interface for PDF text extraction
/// </summary>
public interface IPdfExtractor
{
    /// <summary>
    /// Extract text from a PDF file
    /// </summary>
    Task<string> ExtractTextAsync(Stream pdfStream);

    /// <summary>
    /// Extract text with page information
    /// </summary>
    Task<List<PageContent>> ExtractTextWithPagesAsync(Stream pdfStream);
}

public class PageContent
{
    public int PageNumber { get; set; }
    public string Text { get; set; } = string.Empty;
}
