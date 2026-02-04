using Azure.AI.OpenAI;
using RagWorkshop.Ingestion.Interfaces;
using RagWorkshop.Repository.Models;

namespace RagWorkshop.Ingestion.Services;

/// <summary>
/// Azure OpenAI embedding generator
/// MODULE 1: You'll implement this to generate embeddings
/// </summary>
public class AzureOpenAIEmbeddingGenerator : IEmbeddingGenerator
{
    private readonly OpenAIClient _openAIClient;
    private readonly string _deploymentName;

    public AzureOpenAIEmbeddingGenerator(OpenAIClient openAIClient, string deploymentName)
    {
        _openAIClient = openAIClient ?? throw new ArgumentNullException(nameof(openAIClient));
        _deploymentName = deploymentName;
    }

    /// <summary>
    /// TODO - MODULE 1: Generate embeddings for chunks
    /// Use _openAIClient.GetEmbeddingsAsync() to generate embeddings
    /// Process in batches to handle rate limits
    /// </summary>
    public async Task GenerateEmbeddingsAsync(List<DocumentChunk> chunks)
    {
        // TODO: Implement this method in Module 1
        throw new NotImplementedException("GenerateEmbeddingsAsync - to be implemented in Module 1");
    }
}
