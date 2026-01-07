# RagWorkshop.Repository

Data access layer for document storage and retrieval using Elasticsearch.

## Purpose

Provides a clean abstraction over Elasticsearch for:
- **CRUD Operations**: Save, retrieve, and delete documents
- **Semantic Search**: Vector similarity search using embeddings

## Components

### Interfaces
- `IDocumentRepository` - Repository pattern interface for document operations

### Models
- `Document` - Document with metadata and chunks
- `DocumentChunk` - Text chunk with embedding and metadata
- `SearchResult` - Search result with chunk and relevance score

### Services
- `ElasticsearchDocumentRepository` - Elasticsearch implementation

## Operations

### CRUD
- `SaveDocumentChunksAsync` - Index document chunks
- `GetDocumentAsync` - Retrieve document by ID
- `GetAllDocumentsAsync` - List all documents
- `DeleteDocumentAsync` - Delete document and all its chunks

### Search
- `SearchAsync` - Semantic search using vector similarity
  - Uses Elasticsearch kNN query
  - Returns top-K results above minimum score threshold
  - Supports cosine similarity for vector comparison

## Dependencies

- **Elastic.Clients.Elasticsearch**: Elasticsearch .NET client (v8.11.0)

## Elasticsearch Index

**Index**: `rag-documents`  
**Mapping**:
- Standard text fields (documentId, text, metadata, etc.)
- Dense vector field: `embedding` (1536 dimensions, cosine similarity)

## Usage

Used by:
- **RagWorkshop.Ingestion**: For saving processed documents
- **RagWorkshop.Rag**: For semantic search operations

This shared repository pattern ensures both modules use consistent data access logic.
