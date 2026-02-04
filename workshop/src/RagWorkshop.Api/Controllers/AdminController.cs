using Microsoft.AspNetCore.Mvc;
using Elastic.Clients.Elasticsearch;
using Azure.AI.OpenAI;

namespace RagWorkshop.Api.Controllers;

/// <summary>
/// Admin controller for system health checks
/// MODULE 0: You'll use this to verify your setup
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly ILogger<AdminController> _logger;
    private readonly ElasticsearchClient _elasticsearchClient;
    private readonly OpenAIClient? _openAIClient;
    private readonly IConfiguration _configuration;

    public AdminController(
        ILogger<AdminController> logger,
        ElasticsearchClient elasticsearchClient,
        IConfiguration configuration,
        OpenAIClient? openAIClient = null)
    {
        _logger = logger;
        _elasticsearchClient = elasticsearchClient;
        _openAIClient = openAIClient;
        _configuration = configuration;
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }

    /// <summary>
    /// Check Elasticsearch connection
    /// </summary>
    [HttpGet("elasticsearch/health")]
    public async Task<IActionResult> GetElasticsearchHealth()
    {
        try
        {
            var pingResponse = await _elasticsearchClient.PingAsync();
            return Ok(new
            {
                status = pingResponse.IsValidResponse ? "connected" : "disconnected",
                url = _configuration["Elasticsearch:Url"]
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = "error", message = ex.Message });
        }
    }

    /// <summary>
    /// Check Azure OpenAI connection
    /// </summary>
    [HttpGet("azure-openai/health")]
    public IActionResult GetAzureOpenAIHealth()
    {
        if (_openAIClient == null)
        {
            return Ok(new
            {
                status = "not_configured",
                message = "Azure OpenAI client is not configured"
            });
        }

        return Ok(new
        {
            status = "configured",
            endpoint = _configuration["AzureOpenAI:Endpoint"]
        });
    }
}
