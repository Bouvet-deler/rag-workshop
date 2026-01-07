using Microsoft.AspNetCore.Mvc;
using Elastic.Clients.Elasticsearch;
using Azure.AI.OpenAI;

namespace RagWorkshop.Api.Controllers;

/// <summary>
/// Controller for admin operations - connection checks and diagnostics
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
        OpenAIClient? openAIClient,
        IConfiguration configuration)
    {
        _logger = logger;
        _elasticsearchClient = elasticsearchClient;
        _openAIClient = openAIClient;
        _configuration = configuration;
    }

    /// <summary>
    /// Check Elasticsearch connection
    /// </summary>
    [HttpGet("elasticsearch/health")]
    public async Task<IActionResult> CheckElasticsearch()
    {
        try
        {
            var response = await _elasticsearchClient.PingAsync();

            if (response.IsValidResponse)
            {
                var clusterInfo = await _elasticsearchClient.InfoAsync();
                return Ok(new
                {
                    status = "healthy",
                    connected = true,
                    clusterName = clusterInfo.ClusterName,
                    version = clusterInfo.Version?.Number
                });
            }

            return StatusCode(503, new { status = "unhealthy", connected = false, error = "Elasticsearch not responding" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to Elasticsearch");
            return StatusCode(503, new { status = "error", connected = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Check Azure OpenAI configuration
    /// </summary>
    [HttpGet("azure-openai/health")]
    public IActionResult CheckAzureOpenAI()
    {
        if (_openAIClient == null)
        {
            return StatusCode(503, new
            {
                status = "not_configured",
                connected = false,
                message = "Azure OpenAI not configured. Add credentials to appsettings.json"
            });
        }

        var endpoint = _configuration["AzureOpenAI:Endpoint"];
        var deploymentName = _configuration["AzureOpenAI:DeploymentName"];
        var embeddingDeploymentName = _configuration["AzureOpenAI:EmbeddingDeploymentName"];

        return Ok(new
        {
            status = "configured",
            connected = true,
            endpoint,
            deploymentName,
            embeddingDeploymentName,
            message = "Azure OpenAI client is configured"
        });
    }

    /// <summary>
    /// Get system status - Elasticsearch connection
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetSystemStatus()
    {
        var status = new
        {
            timestamp = DateTime.UtcNow,
            elasticsearch = new { healthy = false, message = "" },
            azureOpenAI = new { configured = false, message = "" }
        };

        try
        {
            var esResponse = await _elasticsearchClient.PingAsync();
            status = status with
            {
                elasticsearch = new
                {
                    healthy = esResponse.IsValidResponse,
                    message = esResponse.IsValidResponse ? "Connected" : "Not responding"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check Elasticsearch");
            status = status with
            {
                elasticsearch = new { healthy = false, message = ex.Message }
            };
        }

        status = status with
        {
            azureOpenAI = new
            {
                configured = _openAIClient != null,
                message = _openAIClient != null ? "Configured" : "Not configured"
            }
        };

        var overallHealthy = status.elasticsearch.healthy && status.azureOpenAI.configured;
        return overallHealthy ? Ok(status) : StatusCode(503, status);
    }

    /// <summary>
    /// List Elasticsearch indices
    /// </summary>
    [HttpGet("elasticsearch/indices")]
    public async Task<IActionResult> GetIndices()
    {
        try
        {
            var response = await _elasticsearchClient.Indices.GetAsync(new Elastic.Clients.Elasticsearch.IndexManagement.GetIndexRequest("*"));

            if (response.IsValidResponse)
            {
                return Ok(new
                {
                    indices = response.Indices.Keys.Select(k => k.ToString()).ToList()
                });
            }

            return StatusCode(500, new { error = "Failed to retrieve indices" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get indices");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
