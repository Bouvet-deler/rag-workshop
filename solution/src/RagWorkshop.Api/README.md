# RagWorkshop.Api

ASP.NET Core Web API that exposes HTTP endpoints for the RAG workshop.

## Running the API

```bash
cd src/RagWorkshop.Api
dotnet run
```

API: http://localhost:5001  
Swagger UI: http://localhost:5001 (opens automatically)

## Configuration

Configure Azure OpenAI in `appsettings.json` or via User Secrets (recommended):

```bash
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://your-resource.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:ApiKey" "your-api-key"
```

Default deployment names:
- Chat: `gpt-4o-mini`
- Embeddings: `text-embedding-3-small`

## API Endpoints

### Admin
- `GET /api/admin/status` - Overall system health
- `GET /api/admin/elasticsearch/health` - Elasticsearch status
- `GET /api/admin/elasticsearch/indices` - List indices
- `GET /api/admin/azure-openai/health` - Azure OpenAI status

### Ingestion (Module 1)
- `POST /api/ingestion/upload` - Process and index a PDF document

### RAG (Module 2)
- `POST /api/rag/search` - Semantic search for document chunks
- `POST /api/rag/chat` - Generate answers using RAG

## Project Dependencies

- **RagWorkshop.Ingestion**: PDF processing pipeline
- **RagWorkshop.Rag**: RAG logic (search + generation)
- **RagWorkshop.Repository**: Data access layer

The API orchestrates these components and exposes them via HTTP.
