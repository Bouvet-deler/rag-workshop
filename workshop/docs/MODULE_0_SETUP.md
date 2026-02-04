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
cd workshop/src/RagWorkshop.Api

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

## Step 3: Build the Basic API Structure

Now let's build [Program.cs](../src/RagWorkshop.Api/Program.cs) - the entry point of your API.

### 📝 Edit `src/RagWorkshop.Api/Program.cs`

Replace the entire file with this code:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "RAG Workshop API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RAG Workshop API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
}));

app.Run();
```

### ✅ Test It

```bash
cd workshop/src/RagWorkshop.Api
dotnet run
```

Open [http://localhost:5001](http://localhost:5001) - you should see the Swagger UI!

Test the health endpoint:
```bash
curl http://localhost:5001/health
```

---

## Step 4: Configure Elasticsearch Connection

Now let's wire up Elasticsearch. Create the extension method to configure the Elasticsearch client.

### 📝 Edit `src/RagWorkshop.Api/Extensions/ElasticsearchServiceExtensions.cs`

Replace the entire file:

```csharp
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using RagWorkshop.Repository.Settings;

namespace RagWorkshop.Api.Extensions;

public static class ElasticsearchServiceExtensions
{
    public static IServiceCollection AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind Elasticsearch settings from configuration
        var elasticsearchSettings = configuration.GetSection("Elasticsearch").Get<ElasticsearchSettings>()
            ?? new ElasticsearchSettings();

        services.Configure<ElasticsearchSettings>(configuration.GetSection("Elasticsearch"));

        // Create Elasticsearch client
        var settings = new ElasticsearchClientSettings(new Uri(elasticsearchSettings.Url))
            .DisableDirectStreaming() // Helpful for debugging
            .RequestTimeout(TimeSpan.FromSeconds(60));

        var client = new ElasticsearchClient(settings);
        services.AddSingleton(client);

        return services;
    }
}
```

### 📝 Update `Program.cs`

Add the Elasticsearch configuration to [Program.cs](../src/RagWorkshop.Api/Program.cs). Add this line **after** `AddSwaggerGen()`:

```csharp
using RagWorkshop.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "RAG Workshop API", Version = "v1" });
});

// Add this line:
builder.Services.AddElasticsearch(builder.Configuration);

var app = builder.Build();
// ... rest of the file
```

### ✅ Test Elasticsearch Connection

Restart your API and test the Elasticsearch health check:

```bash
curl http://localhost:5001/api/admin/elasticsearch/health
```

You should see `"status": "connected"`.

---

## Step 5: Configure Azure OpenAI Connection

Now let's configure Azure OpenAI connectivity.

### 📝 Edit `src/RagWorkshop.Api/Extensions/AzureOpenAIServiceExtensions.cs`

Replace the entire file:

```csharp
using Azure;
using Azure.AI.OpenAI;

namespace RagWorkshop.Api.Extensions;

public static class AzureOpenAIServiceExtensions
{
    public static IServiceCollection AddAzureOpenAI(this IServiceCollection services, IConfiguration configuration)
    {
        var endpoint = configuration["AzureOpenAI:Endpoint"];
        var apiKey = configuration["AzureOpenAI:ApiKey"];

        if (!string.IsNullOrWhiteSpace(endpoint) && !string.IsNullOrWhiteSpace(apiKey))
        {
            var client = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
            services.AddSingleton(client);
        }
        else
        {
            // Register a null client if not configured (for testing without Azure OpenAI)
            services.AddSingleton<OpenAIClient>(provider => null!);
        }

        return services;
    }
}
```

### 📝 Update `Program.cs`

Add Azure OpenAI configuration after the Elasticsearch line:

```csharp
builder.Services.AddElasticsearch(builder.Configuration);
builder.Services.AddAzureOpenAI(builder.Configuration);  // Add this line
```

### ✅ Test Azure OpenAI Connection

Restart your API and test:

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
