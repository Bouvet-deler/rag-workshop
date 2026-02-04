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
