using RagWorkshop.Ingestion.Models;

namespace RagWorkshop.Ingestion.Interfaces;

/// <summary>
/// Interface for document indexing in Elasticsearch
/// </summary>
public interface IDocumentIndexer
{
    /// <summary>
    /// Index a document with its chunks
    /// </summary>
    Task<bool> IndexDocumentAsync(Document document);

    /// <summary>
    /// Delete a document and its chunks
    /// </summary>
    Task<bool> DeleteDocumentAsync(string documentId);

    /// <summary>
    /// Get document by ID
    /// </summary>
    Task<Document?> GetDocumentAsync(string documentId);
}
