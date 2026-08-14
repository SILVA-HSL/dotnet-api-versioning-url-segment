
using Asp.Versioning;
using Microsoft.OpenApi;
using Asp.Versioning.ApiExplorer;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddApiVersioning(options =>
{
    // Fallback to v1.0 if the client sends no version
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    
    // HTTP response headers will include 'api-supported-versions'
    options.ReportApiVersions = true;

    // Use URL segment versioning (e.g., /api/v1/...)
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})

.AddApiExplorer(options =>
 {
     // Format the version as "'v'major[.minor]" (e.g., v1, v2)
     options.GroupNameFormat = "'v'VVV";
     options.SubstituteApiVersionInUrl = true;
 });

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Web API", 
        Version = "v1" 
    });
    c.SwaggerDoc("v2", new OpenApiInfo 
    { 
        Title = "Web API", 
        Version = "v2" 
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web API v1");
        c.SwaggerEndpoint("/swagger/v2/swagger.json", "Web API v2");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
