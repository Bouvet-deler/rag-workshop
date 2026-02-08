using Microsoft.AspNetCore.Mvc;
using RagWorkshop.Rag.Interfaces;

namespace RagWorkshop.Api.Controllers;

/// <summary>
/// Controller for RAG operations - search and chat
/// MODULE 2: You'll implement semantic search and RAG chat
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RagController : ControllerBase
{
    private readonly ILogger<RagController> _logger;
    private readonly IRagService _ragService;

    public RagController(
        ILogger<RagController> logger,
        IRagService ragService)
    {
        _logger = logger;
        _ragService = ragService;
    }

    /// <summary>
    /// TODO - MODULE 2: Semantic search endpoint
    /// Search for relevant document chunks using semantic similarity
    /// </summary>
    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] SearchRequest request)
    {
        // TODO: Implement this endpoint in Module 2
        throw new NotImplementedException("Search - to be implemented in Module 2");
    }

    /// <summary>
    /// TODO - MODULE 2: RAG chat endpoint
    /// Answer questions using retrieved context
    /// </summary>
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        // TODO: Implement this endpoint in Module 2
        throw new NotImplementedException("Chat - to be implemented in Module 2");
    }
}

public record ChatRequest(string Question, int? TopK = 5, string? ConversationId = null);
public record SearchRequest(string Query, int? TopK = 5, float? MinScore = 0.7f);