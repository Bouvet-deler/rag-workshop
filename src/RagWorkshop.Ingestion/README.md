# RagWorkshop.Ingestion

Document processing pipeline for the RAG workshop.

## Purpose

Handles the complete ingestion pipeline:
1. **PDF Extraction**: Extract text from PDF files (with page numbers)
2. **Text Chunking**: Split text into overlapping chunks
3. **Embedding Generation**: Create vector embeddings using Azure OpenAI
4. **Storage**: Save chunks via Repository

## Components

### Interfaces
- `IPdfExtractor` - Extract text from PDF files
- `ITextChunker` - Split text into manageable chunks
- `IEmbeddingGenerator` - Generate embeddings using Azure OpenAI

### Services
- `PdfExtractor` - Uses UglyToad.PdfPig for PDF text extraction
- `SimpleTextChunker` - Chunks text with 500 char size and 50 char overlap
- `AzureOpenAIEmbeddingGenerator` - Generates embeddings via Azure OpenAI
- `IngestionService` - Orchestrates the complete pipeline

## Pipeline Flow

```
PDF File → PdfExtractor → SimpleTextChunker → AzureOpenAIEmbeddingGenerator → Repository.Save
```

## Dependencies

- **RagWorkshop.Repository**: For saving document chunks to Elasticsearch
- **UglyToad.PdfPig**: PDF text extraction
- **Azure.AI.OpenAI**: Embedding generation

## Usage

The `IngestionService` is injected into the API's `IngestionController` and called when PDFs are uploaded.
