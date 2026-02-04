namespace RagWorkshop.Repository.Settings;

/// <summary>
/// Configuration settings for Elasticsearch
/// </summary>
public class ElasticsearchSettings
{
    /// <summary>
    /// Elasticsearch server URL
    /// </summary>
    public string Url { get; set; } = "http://localhost:9200";

    /// <summary>
    /// Default index name for document storage
    /// </summary>
    public string DefaultIndex { get; set; } = "rag-documents";
}
