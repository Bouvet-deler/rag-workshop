using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using RagWorkshop.Ingestion.Interfaces;

namespace RagWorkshop.Ingestion.Services;

/// <summary>
/// PDF text extraction using iText7
/// MODULE 1: You'll implement this to extract text from PDFs
/// </summary>
public class PdfExtractor : IPdfExtractor
{
    /// <summary>
    /// TODO - MODULE 1: Extract all text from a PDF
    /// Use PdfDocument and PdfTextExtractor to read text from all pages
    /// </summary>
    public async Task<string> ExtractTextAsync(Stream pdfStream)
    {
        // TODO: Implement this method in Module 1
        throw new NotImplementedException("ExtractTextAsync - to be implemented in Module 1");
    }

    /// <summary>
    /// TODO - MODULE 1: Extract text with page numbers
    /// Similar to ExtractTextAsync but return a List<PageContent> with page information
    /// </summary>
    public async Task<List<PageContent>> ExtractTextWithPagesAsync(Stream pdfStream)
    {
        // TODO: Implement this method in Module 1
        throw new NotImplementedException("ExtractTextWithPagesAsync - to be implemented in Module 1");
    }
}
