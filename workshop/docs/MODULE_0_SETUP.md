# Module 0: Setup & Configuration ⚙️

**Duration:** ~30 minutes

In this module, you'll set up your development environment and configure the basic API structure with Elasticsearch and Azure OpenAI connectivity.

## 🎯 Learning Objectives

By the end of this module, you will:
- Configure Elasticsearch connection
- Configure Azure OpenAI credentials
- Build a basic ASP.NET Core Web API with Swagger
- Verify all services are connected properly

---

## Step 1: Start Elasticsearch with Docker

First, make sure you have Docker running, then start Elasticsearch:

```bash
# From the root of the repository
cd rag-workshop
docker-compose up -d
```

Wait about 30 seconds for Elasticsearch to fully start, then verify:

```bash
curl http://localhost:9200
```

You should see JSON output with Elasticsearch version information.

### 🌐 Access Elasticvue (Optional)

Elasticvue provides a web UI for Elasticsearch:
- Open [http://localhost:8080](http://localhost:8080)
- Click **"Add elasticsearch cluster"**
- Enter: `http://elasticsearch:9200`
- Click **"Connect"**

---

## Step 2: Configure Azure OpenAI Credentials

You need an Azure OpenAI resource with two deployments:
- **gpt-4o-mini** (for chat completion)
- **text-embedding-3-small** (for generating embeddings)

### Option A: User Secrets (Recommended for Development)

```bash
cd src/RagWorkshop.Api

dotnet user-secrets set "AzureOpenAI:Endpoint" "https://YOUR-RESOURCE.openai.azure.com/"
dotnet user-secrets set "AzureOpenAI:ApiKey" "your-api-key-here"
```

### Option B: appsettings.json (Not recommended - don't commit credentials!)

Edit [appsettings.json](../src/RagWorkshop.Api/appsettings.json) and add your credentials:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://YOUR-RESOURCE.openai.azure.com/",
    "ApiKey": "your-api-key-here",
    "DeploymentName": "gpt-4o-mini",
    "EmbeddingDeploymentName": "text-embedding-3-small"
  }
}
```

---

### Step 3: ✅ Run and test the API 

```bash
cd src/RagWorkshop.Api
dotnet run
```

Open [http://localhost:5001](http://localhost:5001) - you should see the Swagger UI!

Test the health endpoint:
```bash
curl http://localhost:5001/health
```

---

## Step 4: Configure Elasticsearch Connection and Azure Open AI

Now let's wire up Elasticsearch and Azure Open AI.

### 📝 See `src/RagWorkshop.Api/Extensions`

### 📝 Update `Program.cs`

Add ElasticSearch and Azure OpenAI configuration after the Elasticsearch line:

```csharp
using RagWorkshop.Api.Extensions; // Add this line

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "RAG Workshop API", Version = "v1" });
});

builder.Services.AddElasticsearch(builder.Configuration); // Add this line
builder.Services.AddAzureOpenAI(builder.Configuration);  // Add this line

// ... rest of the file
```

Restart your API and test the Elasticsearch health check

### ✅ Test Elasticsearch Connection

```bash
curl http://localhost:5001/api/admin/elasticsearch/health
```

You should see `"status": "connected"`.

### ✅ Test Azure OpenAI Connection


```bash
curl http://localhost:5001/api/admin/azure-openai/health
```

You should see `"status": "configured"`.

---

## 🎉 Module 0 Complete!

You've successfully set up:
- ✅ Elasticsearch running in Docker
- ✅ Basic ASP.NET Core Web API with Swagger
- ✅ Elasticsearch connection configured
- ✅ Azure OpenAI connection configured
- ✅ Admin endpoints for health checks

### 🧪 Final Verification

Test all endpoints:

```bash
# General health
curl http://localhost:5001/health

# Overall status
curl http://localhost:5001/api/admin/status

# Elasticsearch health
curl http://localhost:5001/api/admin/elasticsearch/health

# Azure OpenAI health
curl http://localhost:5001/api/admin/azure-openai/health
```

All should return successful responses!

---

## 📚 What You Learned

- How to set up Elasticsearch with Docker
- How to configure ASP.NET Core Web API with Swagger
- How to use dependency injection for service configuration
- How to externalize configuration using appsettings.json and user secrets
- How to create health check endpoints

---

## 🚀 Next Steps

**[Continue to Module 1: Document Ingestion Pipeline →](MODULE_1_INGESTION.md)**

In Module 1, you'll build the complete pipeline to ingest PDF documents, extract text, chunk it, generate embeddings, and store everything in Elasticsearch!
