# RAG Workshop - Development Environment Setup

A hands-on workshop for building a RAG (Retrieval-Augmented Generation) application with PDF upload and chat capabilities.

## Prerequisites

Before starting, ensure you have the following installed:

### All Platforms
- **Docker Desktop** (or Docker Engine + Docker Compose on Linux)
  - Windows: [Download Docker Desktop for Windows](https://www.docker.com/products/docker-desktop/)
  - macOS: [Download Docker Desktop for Mac](https://www.docker.com/products/docker-desktop/) (supports both Intel and Apple Silicon)
  - Linux: [Install Docker Engine](https://docs.docker.com/engine/install/) + [Docker Compose](https://docs.docker.com/compose/install/)

- **.NET 8 SDK**
  - [Download .NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
  - Verify installation: `dotnet --version` (should show 8.x.x)

## Quick Start

### 1. Clone the Repository
```bash
git clone <your-repo-url>
cd rag
```

### 2. Start Elasticsearch
```bash
docker-compose up -d
```

This will start:
- **Elasticsearch** on `http://localhost:9200`
- **Elasticvue** (UI) on `http://localhost:8080`

### 3. Verify It's Working

Wait about 30 seconds for Elasticsearch to start, then test:

```bash
curl http://localhost:9200
```

You should see a JSON response with Elasticsearch version information.

### 4. Connect Elasticvue to Elasticsearch

1. Open Elasticvue in your browser: http://localhost:8080
2. You'll see the welcome page - click **"Add elasticsearch cluster"**
3. Configure the connection:
   - **Cluster Name**: `rag-workshop` (or any name you like)
   - **Uri**: `http://localhost:9200`
   - **Authorization**: Select **"No authorization"**
4. Click **"Test connection"** - you should see a success message
5. Click **"Connect"** to save and connect

You should now see your Elasticsearch cluster in Elasticvue!

### 5. Configure Azure OpenAI (Required for Modules 1 & 2)

To use Azure OpenAI for embeddings and chat, configure your credentials in `appsettings.json` or using User Secrets:

```bash
cd src/RagWorkshop.Api

# Set your Azure OpenAI endpoint (from Azure portal)
dotnet user-secrets set "AzureOpenAI:Endpoint" "https://your-resource.openai.azure.com/"

# Set your Azure OpenAI API key
dotnet user-secrets set "AzureOpenAI:ApiKey" "your-api-key-here"

# Optional: Override deployment names (defaults are gpt-4o-mini and text-embedding-3-small)
dotnet user-secrets set "AzureOpenAI:DeploymentName" "gpt-4o-mini"
dotnet user-secrets set "AzureOpenAI:EmbeddingDeploymentName" "text-embedding-3-small"
```

Verify your configuration:
```bash
dotnet user-secrets list
```

**Note**: User secrets override `appsettings.json` and are never committed to source control.

### 6. Run the API

```bash
cd src/RagWorkshop.Api
dotnet run
```

The API will start at http://localhost:5001 with Swagger UI at the root.

**Test your connections**: Visit http://localhost:5001 and try the `/api/admin/status` endpoint to verify both Elasticsearch and Azure OpenAI are configured correctly.

## Project Structure

```
rag/
├── docker-compose.yml          # Elasticsearch and Elasticvue services
├── src/
│   ├── RagWorkshop.Api/        # ASP.NET Core Web API (HTTP layer)
│   │   ├── Controllers/
│   │   │   ├── AdminController.cs        # Connection checks & diagnostics
│   │   │   ├── IngestionController.cs    # Module 1: PDF upload & indexing
│   │   │   └── RagController.cs          # Module 2: RAG chat & search
│   │   ├── appsettings.json              # Configuration
│   │   └── Program.cs                    # DI setup & app startup
│   ├── RagWorkshop.Ingestion/  # Module 1: Document processing pipeline
│   │   ├── Interfaces/         # IPdfExtractor, ITextChunker, IEmbeddingGenerator
│   │   └── Services/           # PDF extraction, chunking, embedding generation
│   ├── RagWorkshop.Rag/        # Module 2: RAG logic (search + generation)
│   │   ├── Interfaces/         # IRagService
│   │   └── Services/           # Semantic search & answer generation
│   └── RagWorkshop.Repository/ # Data access layer (CRUD + Search)
│       ├── Interfaces/         # IDocumentRepository
│       ├── Models/             # Document, DocumentChunk, SearchResult
│       └── Services/           # Elasticsearch operations
└── README.md
```

## Architecture

The solution follows a **layered architecture** with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────┐
│                   RagWorkshop.Api                       │
│              (Controllers & HTTP Layer)                 │
└──────────────┬─────────────────────┬────────────────────┘
               │                     │
               ▼                     ▼
┌──────────────────────────┐ ┌────────────────────────────┐
│  RagWorkshop.Ingestion   │ │     RagWorkshop.Rag        │
│  (PDF → Chunks →         │ │  (Search → Context →       │
│   Embeddings)            │ │   Generation)              │
└──────────────┬───────────┘ └─────────────┬──────────────┘
               │                           │
               │                           │
               └───────────┬───────────────┘
                           ▼
               ┌───────────────────────────┐
               │  RagWorkshop.Repository   │
               │  (CRUD + Search)          │
               └───────────┬───────────────┘
                           │
                           ▼
                   ┌───────────────┐
                   │ Elasticsearch │
                   │  (Vector DB)  │
                   └───────────────┘
```

### Dependency Flow
- **Api** → Ingestion, Rag
- **Ingestion** → Repository (for CRUD operations)
- **Rag** → Repository (for search operations)
- **Repository** → Elasticsearch

This architecture ensures:
- **Separation of Concerns**: Each project has a single, well-defined responsibility
- **Testability**: Business logic is isolated from infrastructure
- **Reusability**: Repository is shared by both Ingestion and RAG
- **Workshop Clarity**: Each module can be taught independently

## Workshop Modules

### Admin Endpoints
- `GET /api/admin/status` - Check system status (Elasticsearch + Azure OpenAI)
- `GET /api/admin/elasticsearch/health` - Elasticsearch cluster health
- `GET /api/admin/elasticsearch/indices` - List all indices
- `GET /api/admin/azure-openai/health` - Azure OpenAI connection check

### Module 1: Document Ingestion
Upload and process PDF documents into searchable chunks with embeddings.

**Endpoints**:
- `POST /api/ingestion/upload` - Upload PDF (currently uses hardcoded file path)

**Pipeline**: PDF → Text Extraction → Chunking → Embedding Generation → Elasticsearch Indexing

### Module 2: RAG (Retrieval-Augmented Generation)
Search for relevant context and generate AI-powered answers.

**Endpoints**:
- `POST /api/rag/search` - Semantic search for document chunks
- `POST /api/rag/chat` - Generate answers with RAG

**Pipeline**: Query → Embedding → Vector Search → Context Building → LLM Generation

## Common Commands

```bash
# Start services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down

# Stop and remove all data (fresh start)
docker-compose down -v
```

## Troubleshooting

### Elasticsearch won't start

**Issue**: Container exits immediately or won't start

**Solution**: 
```bash
# Stop everything
docker-compose down -v

# Check Docker is running
docker ps

# Start with logs visible
docker-compose up
```

### Port already in use

**Issue**: `Error: port 9200 is already allocated`

**Solution**: Another service is using port 9200
```bash
# Find what's using the port
# Windows
netstat -ano | findstr :9200

# macOS/Linux
lsof -i :9200

# Kill the process or change the port in docker-compose.yml
```

### Elasticvue can't connect

**Issue**: Elasticvue UI loads but can't connect to Elasticsearch

**Solution**: 
1. Make sure Elasticsearch is running: `curl http://localhost:9200`
2. In Elasticvue, use `http://localhost:9200` (NOT `http://elasticsearch:9200`)

### Docker Desktop not starting (Windows)

**Issue**: WSL 2 installation incomplete

**Solution**:
1. Install WSL 2: `wsl --install`
2. Restart your computer
3. Open Docker Desktop

## Verifying Your Setup

Run this quick check:

```bash
# Check Elasticsearch is responding
curl http://localhost:9200

# Create a test document
curl -X POST "http://localhost:9200/test-index/_doc/1" \
  -H "Content-Type: application/json" \
  -d '{"message": "Hello RAG Workshop!"}'

# Retrieve it
curl http://localhost:9200/test-index/_doc/1

# View in Elasticvue: http://localhost:8080
```

## Next Steps

Once Elasticsearch is running successfully, you're ready for the workshop! We'll build:

1. PDF upload and chunking
2. Azure OpenAI embeddings
3. Vector search with Elasticsearch
4. Chat interface with RAG

## Need Help?

- Check logs: `docker-compose logs -f elasticsearch`
- Verify Docker: `docker ps`
- Clean restart: `docker-compose down -v && docker-compose up -d`

---

**Workshop Ready?** ✅ If you can access http://localhost:9200 and http://localhost:8080, you're all set!
