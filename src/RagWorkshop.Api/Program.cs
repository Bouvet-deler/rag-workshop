using RagWorkshop.Api.Extensions;
using RagWorkshop.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "RAG Workshop API", Version = "v1" });
});

// Configure application services
builder.Services.AddElasticsearch(builder.Configuration);
builder.Services.AddAzureOpenAI(builder.Configuration);
builder.Services.AddIngestionServices(builder.Configuration);
builder.Services.AddRagServices(builder.Configuration);

var app = builder.Build();

// Initialize Elasticsearch index with proper mapping
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<ElasticsearchInitializer>();
    await initializer.InitializeAsync();
}

// Configure the HTTP request pipeline.
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
