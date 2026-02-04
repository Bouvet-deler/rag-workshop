# RAG Workshop - Build Your Own Retrieval-Augmented Generation System

A hands-on workshop where you'll build a complete RAG (Retrieval-Augmented Generation) application from scratch with PDF upload, semantic search, and AI-powered chat capabilities.

## 🎯 Two Ways to Use This Repository

### 🎓 **Interactive Workshop** (Recommended for Learning)
**[Start the Workshop →](workshop/README.md)**

Build the RAG system step-by-step by following guided instructions and copying code snippets. Perfect for learning how RAG works from the ground up.
- **Duration:** ~2.5 hours
- **Modules:** Setup → Ingestion Pipeline → RAG Implementation
- **What you'll build:** PDF processing, embeddings, vector search, AI chat

### ⚡ **Complete Solution** (Ready to Run)
**[View the Solution →](solution/README.md)**

A fully working RAG implementation you can run immediately. Use it as reference, for quick demos, or to see how everything fits together.
- Run immediately after configuring credentials
- Study the complete implementation
- Use as reference while doing the workshop

---

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

Choose your path:

### 🎓 **Workshop Mode** (Learn by Building)

**[→ Start the Interactive Workshop](workshop/README.md)**

1. Start Elasticsearch
2. Follow Module 0 to set up the API
3. Build the ingestion pipeline (Module 1)
4. Implement RAG (Module 2)

### ⚡ **Solution Mode** (Quick Demo)

**[→ Run the Complete Solution](solution/README.md)**

1. Start Elasticsearch (see below)
2. Configure Azure OpenAI credentials
3. Run `dotnet run --project src/RagWorkshop.Api` from solution folder
4. Access Swagger UI at http://localhost:5001

---

## Infrastructure Setup (Required for Both)

### 1. Start Elasticsearch
```bash
docker-compose up -d
```

This will start:
- **Elasticsearch** on `http://localhost:9200`
- **Elasticvue** (UI) on `http://localhost:8080`

### 2. Verify Elasticsearch

Wait about 30 seconds for Elasticsearch to start, then test:

```bash
curl http://localhost:9200
```

You should see a JSON response with Elasticsearch version information.

### 3. Access Elasticvue (Optional UI)

1. Open Elasticvue in your browser: http://localhost:8080
2. You'll see the welcome page - click **"Add elasticsearch cluster"**
3. Configure the connection:
   - **Cluster Name**: `rag-workshop` (or any name you like)
   - **Uri**: `http://localhost:9200`
   - **Authorization**: Select **"No authorization"**
4. Click **"Test connection"** - you should see a success message
5. Click **"Connect"** to save and connect

You should now see your Elasticsearch cluster in Elasticvue!

### 4. Configure Azure OpenAI

You'll need Azure OpenAI with these deployments:
- **gpt-4o-mini** (chat completion)
- **text-embedding-3-small** (embeddings)

**Using User Secrets (Recommended):**

```bash
# Navigate to the API project (choose workshop or solution)
cd workshop/src/RagWorkshop.Api
# OR
cd solution/src/RagWorkshop.Api

dotnet user-secrets set "AzureOpenAI:Endpoint" "https://YOUR-RESOURCE.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:ApiKey" "your-api-key-here"
```

**Or edit appsettings.json** (not recommended for production):
- Update `AzureOpenAI:Endpoint` and `AzureOpenAI:ApiKey` in appsettings.json

---

## 📁 Repository Structure

```
rag-workshop/
├── workshop/                    # 🎓 Interactive learning path
│   ├── README.md               # Workshop overview and navigation
│   ├── docs/                   # Step-by-step module guides
│   │   ├── MODULE_0_SETUP.md
│   │   ├── MODULE_1_INGESTION.md
│   │   └── MODULE_2_RAG.md
│   └── src/                    # Starter code with TODOs
│       ├── RagWorkshop.Api/
│       ├── RagWorkshop.Ingestion/
│       ├── RagWorkshop.Rag/
│       └── RagWorkshop.Repository/
│
├── solution/                    # ⚡ Complete working implementation
│   ├── README.md               # Solution reference guide
│   ├── RagWorkshop.sln         # Solution file
│   └── src/                    # Source code
│       ├── RagWorkshop.Api/
│       ├── RagWorkshop.Ingestion/
│       ├── RagWorkshop.Rag/
│       └── RagWorkshop.Repository/
│
└── docker-compose.yml          # Elasticsearch + Elasticvue
```

---

## 🎓 What You'll Learn

In the **workshop**, you'll learn how to:
- Extract text from PDF files with page tracking
- Implement text chunking strategies with overlap
- Generate vector embeddings using Azure OpenAI
- Store and index embeddings in Elasticsearch
- Perform semantic search using vector similarity (kNN)
- Build the RAG pattern: Retrieval + Augmentation + Generation
- Create REST APIs with ASP.NET Core
- Wire up dependency injection for clean architecture

---

## 🚀 Getting Started

### For Learners (Recommended)
**[👉 Start the Workshop](workshop/README.md)**

Begin with Module 0 and work through each module sequentially, building your understanding and the application together.

### For Quick Exploration
**[👉 Run the Solution](solution/README.md)**

Skip straight to the working implementation to see what you'll build or use as reference material.

---

## 🔧 Technologies Used

| Technology | Purpose |
|------------|---------|
| **ASP.NET Core 8** | REST API framework |
| **Azure OpenAI** | Embeddings (text-embedding-3-small) & Chat (gpt-4o-mini) |
| **Elasticsearch** | Vector database for semantic search |
| **iText7** | PDF text extraction |
| **Docker** | Elasticsearch infrastructure |

---

## 💡 Need Help?

- **Workshop stuck?** Check the `/solution` folder for working implementations
- **API not working?** Use `/api/admin/*` endpoints to diagnose issues
- **Want to compare?** Use `diff` or VS Code's compare feature between workshop and solution

---

## 🎉 What You'll Build

By the end of the workshop, you'll have created:
- ✅ PDF document ingestion pipeline
- ✅ Vector embedding generation and storage
- ✅ Semantic search functionality
- ✅ AI-powered Q&A with source citations
- ✅ Production-ready REST API

All built from scratch with full understanding of each component!

---
```
**Ready to begin? [Start the Workshop →](workshop/README.md)**
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
