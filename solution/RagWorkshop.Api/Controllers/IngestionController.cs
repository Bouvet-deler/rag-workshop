using Microsoft.AspNetCore.Mvc;
using RagWorkshop.Ingestion.Services;
using RagWorkshop.Repository.Interfaces;

namespace RagWorkshop.Api.Controllers;

/// <summary>
/// Controller for document ingestion - PDF upload and indexing
/// Workshop Module 1: Ingestion
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
    /// Upload and process a PDF document from file path
    /// </summary>
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

            using var fileStream = System.IO.File.OpenRead(request.FilePath);
            var document = await _ingestionService.ProcessDocumentAsync(fileStream, request.FilePath);

            return Ok(new
            {
                documentId = document.Id,
                fileName = document.FileName,
                status = document.Status,
                chunksCount = document.Chunks.Count,
                message = "Document processed successfully"
            });
        }
        catch (NotImplementedException)
        {
            return Ok(new
            {
                message = "Ingestion pipeline not yet implemented",
                status = "pending_implementation"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PDF");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get ingestion system status - total documents and chunks
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatus()
    {
        try
        {
            var documents = await _documentRepository.GetAllDocumentsAsync();
            var totalDocuments = documents.Count;
            var totalChunks = documents.Sum(d => d.Chunks.Count);

            return Ok(new
            {
                totalDocuments,
                totalChunks,
                documents = documents.Select(d => new
                {
                    documentId = d.Id,
                    fileName = d.FileName,
                    chunksCount = d.Chunks.Count,
                    uploadedAt = d.UploadedAt,
                    status = d.Status
                }),
                message = "Ingestion system status"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ingestion status");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

public record UploadRequest(string FilePath);
