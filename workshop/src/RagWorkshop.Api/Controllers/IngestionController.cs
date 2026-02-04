using Microsoft.AspNetCore.Mvc;
using RagWorkshop.Ingestion.Services;
using RagWorkshop.Repository.Interfaces;

namespace RagWorkshop.Api.Controllers;

/// <summary>
/// Controller for document ingestion - PDF upload and indexing
/// MODULE 1: You'll implement PDF upload functionality
/// </summary>
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

    /// <summary>
    /// TODO - MODULE 1: Upload and process a PDF document
    /// Accept a file path, read the PDF, and process it through the ingestion pipeline
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadPdf([FromBody] UploadRequest request)
    {
        // TODO: Implement this endpoint in Module 1
        throw new NotImplementedException("UploadPdf - to be implemented in Module 1");
    }
}

public class UploadRequest
{
    public string FilePath { get; set; } = string.Empty;
}
