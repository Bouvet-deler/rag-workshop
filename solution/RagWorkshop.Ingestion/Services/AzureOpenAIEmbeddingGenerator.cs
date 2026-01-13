using Azure.AI.OpenAI;
using RagWorkshop.Ingestion.Interfaces;

namespace RagWorkshop.Ingestion.Services;

/// <summary>
/// Generate embeddings using Azure OpenAI
/// </summary>
public class AzureOpenAIEmbeddingGenerator : IEmbeddingGenerator
{
    private readonly OpenAIClient _client;
    private readonly string _deploymentName;

    public AzureOpenAIEmbeddingGenerator(OpenAIClient client, string deploymentName = "text-embedding-3-small")
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _deploymentName = deploymentName;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        var embeddingsOptions = new EmbeddingsOptions(_deploymentName, new[] { text });
        var response = await _client.GetEmbeddingsAsync(embeddingsOptions);

        return response.Value.Data[0].Embedding.ToArray();
    }

    public async Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts)
    {
        var embeddingsOptions = new EmbeddingsOptions(_deploymentName, texts);
        var response = await _client.GetEmbeddingsAsync(embeddingsOptions);

        return response.Value.Data.Select(d => d.Embedding.ToArray()).ToList();
    }
}
