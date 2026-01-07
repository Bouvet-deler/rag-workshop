# RagWorkshop.Rag

RAG (Retrieval-Augmented Generation) logic for searching documents and generating answers.

## Purpose

Implements the RAG pattern:
1. **Retrieval**: Semantic search for relevant document chunks
2. **Augmentation**: Build context from retrieved chunks
3. **Generation**: Generate answers using Azure OpenAI

## Components

### Interfaces
- `IRagService` - Main RAG service interface

### Services
- `RagService` - RAG implementation with search and generation

### Models
- `RagResponse` - Answer with sources and token usage
- `SourceChunk` - Source information for citations

## RAG Pipeline

```
User Query → Generate Embedding → Vector Search → Build Context → LLM Generation → Answer + Sources
```

### SearchAsync
Performs semantic search:
- Generates query embedding
- Searches via Repository
- Returns ranked results with scores

### GenerateAnswerAsync
Complete RAG pipeline:
1. Search for top-K relevant chunks
2. Build context from retrieved chunks
3. Create system + user prompts
4. Call Azure OpenAI for generation
5. Return answer with source citations

## Dependencies

- **RagWorkshop.Repository**: For semantic search operations
- **Azure.AI.OpenAI**: For embeddings and chat completions

## Usage

Used by `RagController` to expose RAG capabilities via HTTP endpoints.
