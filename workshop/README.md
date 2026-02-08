# RAG Workshop - Build Your Own Retrieval-Augmented Generation System (C# / .NET)

Welcome to the RAG Workshop! In this hands-on workshop, you'll build a complete RAG (Retrieval-Augmented Generation) application from scratch that can:
- 📄 **Ingest PDF documents** and extract their content
- 🔍 **Perform semantic search** to find relevant information
- 💬 **Generate AI-powered answers** based on your documents

## 🎯 What You'll Learn

- How to extract text from PDF files
- Text chunking strategies for better retrieval
- Generating embeddings with Azure OpenAI
- Storing and searching vectors in Elasticsearch
- Building a complete RAG pipeline (Retrieval + Augmentation + Generation)
- Creating a REST API with ASP.NET Core

## 📚 Workshop Structure

The workshop is divided into modules that you'll complete step-by-step:

### **[Module 0: Setup & Configuration](docs/MODULE_0_SETUP.md)** ⚙️
Set up your development environment, configure Elasticsearch and Azure OpenAI, and build the basic API structure.

**Duration:** ~30 minutes

### **[Module 1: Document Ingestion Pipeline](docs/MODULE_1_INGESTION.md)** 📥
Build the complete ingestion pipeline: PDF extraction, text chunking, embedding generation, and storage in Elasticsearch.

**Duration:** ~60 minutes

### **[Module 2: RAG - Search & Generation](docs/MODULE_2_RAG.md)** 🤖
Implement semantic search and answer generation using the RAG pattern.

**Duration:** ~45 minutes

## 🏗️ Architecture

```
┌─────────────┐
│   PDF File  │
└──────┬──────┘
       │
       v
┌─────────────────────────────────────────────────────┐
│         MODULE 1: Ingestion Pipeline                │
│                                                      │
│  PDF Extractor → Text Chunker → Embedding Generator │
└──────────────────────┬──────────────────────────────┘
                       │
                       v
             ┌─────────────────┐
             │  Elasticsearch  │
             │   (Vector DB)   │
             └─────────────────┘
                       │
                       v
┌─────────────────────────────────────────────────────┐
│            MODULE 2: RAG Pipeline                    │
│                                                      │
│  Query → Embedding → Search → Context → GPT Answer  │
└─────────────────────────────────────────────────────┘
```

## 🚀 Quick Start

### Prerequisites

Before you begin, make sure you have:
- ✅ **.NET 8 SDK** ([Download](https://dotnet.microsoft.com/download/dotnet/8.0))
- ✅ **Docker Desktop** ([Download](https://www.docker.com/products/docker-desktop/))
- ✅ **Azure OpenAI access** with `gpt-4o-mini` and `text-embedding-3-small` deployments
- ✅ **VS Code** (recommended) or your favorite IDE

### Get Started

1. **Start with [Module 0: Setup](docs/MODULE_0_SETUP.md)** to configure your environment
2. **Progress through each module sequentially** - each builds on the previous
3. **Copy code snippets** from the READMEs into your project files
4. **Test as you go** using the provided curl commands and Swagger UI

## 📁 Project Structure

```
workshop/
├── src/
│   ├── RagWorkshop.Api/          # REST API (ASP.NET Core)
│   ├── RagWorkshop.Ingestion/    # PDF processing & embedding generation
│   ├── RagWorkshop.Rag/          # RAG service (search + generation)
│   └── RagWorkshop.Repository/   # Elasticsearch data access
└── docs/
    ├── MODULE_0_SETUP.md
    ├── MODULE_1_INGESTION.md
    └── MODULE_2_RAG.md
```

## 🎓 Learning Path

### For Complete Beginners
Follow each module in order, reading the explanations and copying the code snippets. Take your time to understand each concept before moving forward.

### For Experienced Developers
You can move faster through the modules, focusing on the RAG-specific concepts. Feel free to customize and experiment with different chunking strategies, embedding models, or search parameters.

## 🆘 Need Help?

- **Stuck?** Check the `/solution` folder for complete working implementations
- **Errors?** Use the Admin API endpoints (`/api/admin/*`) to diagnose connectivity issues
- **Want to learn more?** Each module README includes "Learn More" sections with additional resources

## 🎉 What You'll Build

By the end of this workshop, you'll have a fully functional RAG system that:

1. **Accepts PDF uploads** and processes them automatically
2. **Stores document chunks** with vector embeddings in Elasticsearch
3. **Performs semantic search** to find relevant information
4. **Generates contextual answers** using GPT-4o-mini

You can then extend this system for your own use cases: document Q&A, knowledge bases, research assistants, and more!

## 🏁 Ready to Begin?

**[👉 Start with Module 0: Setup & Configuration](docs/MODULE_0_SETUP.md)**

---

## 📊 Technologies Used

| Technology | Purpose |
|------------|---------|
| **ASP.NET Core 8** | REST API framework |
| **Azure OpenAI** | Embeddings & chat completion |
| **Elasticsearch** | Vector database for semantic search |
| **iText7** | PDF text extraction |
| **C# / .NET** | Implementation language |

---

Happy building! 🚀
