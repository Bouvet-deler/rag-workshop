# RAG Workshop - Build Your Own Retrieval-Augmented Generation System

## Prerequisites

Before starting, ensure you have the following installed:

### All Platforms
- **Docker or Podman** (container runtime)
  - **Docker** (recommended for beginners):
    - Windows: [Download Docker Desktop for Windows](https://www.docker.com/products/docker-desktop/)
    - macOS: [Download Docker Desktop for Mac](https://www.docker.com/products/docker-desktop/) (supports both Intel and Apple Silicon)
    - Linux: [Install Docker Engine](https://docs.docker.com/engine/install/) + [Docker Compose](https://docs.docker.com/compose/install/)
  - **Podman** (Docker-compatible alternative):
    - Windows: [Download Podman Desktop](https://podman-desktop.io/downloads)
    - macOS: `brew install podman` + [Podman Desktop](https://podman-desktop.io/downloads)
    - Linux: [Install Podman](https://podman.io/getting-started/installation) + `podman-compose`
  - Verify installation: `docker --version` or `podman --version`

- **.NET 8 SDK**
  - [Download .NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
  - Verify installation: `dotnet --version` (should show 8.x.x)

---

## Infrastructure Setup (Required for Both)

### 1. Start Elasticsearch

**Using Docker:**
```bash
docker-compose up -d
```

**Using Podman:**
```bash
podman-compose up -d
# OR (with Podman 4.0+)
podman compose up -d
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

## Quick Start

Now that you have the infrastructure running, choose your path:

### 🎓 **Workshop Mode** (Learn by Building)

**[→ Start the Interactive Workshop](workshop/README.md)**

Follow the step-by-step modules to build the RAG system from scratch:
1. Module 0: Configure the API and verify connections
2. Module 1: Build the document ingestion pipeline
3. Module 2: Implement the RAG functionality

### ⚡ **Solution Mode** (Quick Demo)

**[→ Run the Complete Solution](solution/README.md)**

Run the complete working implementation immediately:
1. Run `dotnet run --project solution/src/RagWorkshop.Api`
2. Access Swagger UI at http://localhost:5001
3. Upload PDFs and start asking questions

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

**Using Docker:**
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

**Using Podman:**
```bash
# Start services
podman-compose up -d
# OR: podman compose up -d

# View logs
podman-compose logs -f

# Stop services
podman-compose down

# Stop and remove all data (fresh start)
podman-compose down -v
```

## Troubleshooting

### Elasticsearch won't start

**Issue**: Container exits immediately or won't start

**Solution**: 
```bash
# Stop everything
docker-compose down -v
# OR: podman-compose down -v

# Check containers are running
docker ps
# OR: podman ps

# Start with logs visible
docker-compose up
# OR: podman-compose up
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
# Note: Same steps apply whether using Docker or Podman
```

### Elasticvue can't connect

**Issue**: Elasticvue UI loads but can't connect to Elasticsearch

**Solution**: 
1. Make sure Elasticsearch is running: `curl http://localhost:9200`
2. In Elasticvue, use `http://localhost:9200` (NOT `http://elasticsearch:9200`)

### Docker Desktop not starting (Windows)

**Issue**: WSL 2 installation incomplete

**Solution** (Docker users):
1. Install WSL 2: `wsl --install`
2. Restart your computer
3. Open Docker Desktop

**Alternative**: Use [Podman Desktop](https://podman-desktop.io/) instead

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
