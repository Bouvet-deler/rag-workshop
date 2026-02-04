using Microsoft.AspNetCore.Mvc;
using RagWorkshop.Rag.Interfaces;

namespace RagWorkshop.Api.Controllers;

/// <summary>
/// Controller for RAG operations - Retrieval, Augmentation, Generation
/// Workshop Module 3: RAG
/// </summary>
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
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Search([FromBody] SearchRequest request)
    {
        _logger.LogInformation("Search endpoint called with query: {Query}", request.Query);

        try
        {
            var results = await _ragService.SearchAsync(request.Query, request.TopK ?? 5, request.MinScore ?? 0.7f);

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
    /// Chat endpoint with RAG capabilities - retrieves context and generates answer
    /// </summary>
    [HttpPost("chat")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
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

public record ChatRequest(string Question, int? TopK = 5, string? ConversationId = null);
public record SearchRequest(string Query, int? TopK = 5, float? MinScore = 0.7f);
