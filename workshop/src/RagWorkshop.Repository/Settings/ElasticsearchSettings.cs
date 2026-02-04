namespace RagWorkshop.Repository.Settings;

/// <summary>
/// Elasticsearch configuration settings
/// </summary>
public class ElasticsearchSettings
{
    public string Url { get; set; } = "http://localhost:9200";
    public string DefaultIndex { get; set; } = "rag-documents";
}
