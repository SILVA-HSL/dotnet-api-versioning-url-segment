# URL-Level API Versioning Guide

## Overview
Your ASP.NET Core Web API is configured for **URL-level API versioning**. This means the API version is specified in the URL path (e.g., `/api/v1/...`, `/api/v2/...`).

## How to Use

### Request Examples
```bash
# Call v1 endpoint
curl https://localhost:5001/api/v1/weatherforecast

# Call v2 endpoint
curl https://localhost:5001/api/v2/weatherforecast
```

### Response Headers
When enabled in configuration, responses include:
```
api-supported-versions: 1.0, 2.0
```

---

## Configuration Details

### 1. Program.cs - API Versioning Setup
```csharp
builder.Services.AddApiVersioning(options =>
{
    // Fallback to v1.0 if no version specified
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    
    // Include supported versions in response headers
    options.ReportApiVersions = true;

    // Read version from URL segment: /api/v{version}/...
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});
```

**What each option does:**
- `DefaultApiVersion` - Falls back to v1.0 if client doesn't specify a version
- `AssumeDefaultVersionWhenUnspecified` - When `true`, missing version uses the default
- `ReportApiVersions` - Adds `api-supported-versions` header to responses
- `UrlSegmentApiVersionReader` - Extracts version from URL (e.g., `/api/v1/...`)

### 2. Controller Configuration
```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]      // Declares controller supports v1.0
[ApiVersion("2.0")]      // Declares controller supports v2.0
public class WeatherForecastController : ControllerBase
{
    [MapToApiVersion("1.0")]
    [HttpGet]
    public IEnumerable<WeatherForecast> GetV1() { ... }

    [MapToApiVersion("2.0")]
    [HttpGet]
    public IEnumerable<WeatherForecast> GetV2() { ... }
}
```

**Route Parameter Breakdown:**
- `api/v{version:apiVersion}` - Version is required and must be a valid `apiVersion`
- `[controller]` - Replaced with controller name (`weatherforecast`)
- `[MapToApiVersion(...)]` - Links method to specific API version

---

## How to Add a New API Version

### Step 1: Declare Support in Controller
```csharp
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[ApiVersion("3.0")]  // ← Add new version
public class WeatherForecastController : ControllerBase { ... }
```

### Step 2: Add Version-Specific Method
```csharp
[MapToApiVersion("3.0")]
[HttpGet]
public IEnumerable<WeatherForecast> GetV3()
{
    // v3 implementation
    return Enumerable.Range(1, 15)...;
}
```

### Step 3: Update Swagger (Program.cs)
```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Web API", Version = "v1" });
    c.SwaggerDoc("v2", new OpenApiInfo { Title = "Web API", Version = "v2" });
    c.SwaggerDoc("v3", new OpenApiInfo { Title = "Web API", Version = "v3" });  // ← Add
});

// In Swagger UI config
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web API v1");
    c.SwaggerEndpoint("/swagger/v2/swagger.json", "Web API v2");
    c.SwaggerEndpoint("/swagger/v3/swagger.json", "Web API v3");  // ← Add
    c.RoutePrefix = "swagger";
});
```

---

## Testing Your API

### Using curl
```bash
# Test v1
curl https://localhost:5001/api/v1/weatherforecast

# Test v2
curl https://localhost:5001/api/v2/weatherforecast
```

### Using Swagger UI
1. Start the app: `dotnet run`
2. Open: `https://localhost:5001/swagger`
3. Select version from dropdown (v1 or v2)
4. Click "Try it out" to test

### Using REST Client Extension (.http file)
Create `test-api.http` in project root:
```http
@baseUrl = https://localhost:5001

### Get Weather Forecast - v1
GET {{baseUrl}}/api/v1/weatherforecast

### Get Weather Forecast - v2
GET {{baseUrl}}/api/v2/weatherforecast
```

---

## URL vs. Other Versioning Strategies

| Strategy | Example | Pros | Cons |
|----------|---------|------|------|
| **URL (Current)** | `/api/v1/resource` | Clear, cacheable, visible in logs | Longer URLs |
| Header | `Header: api-version=1` | Clean URLs | Hidden from browser |
| Query String | `/api/resource?version=1` | Flexible | Can break caching |
| Custom Header + URL | Both methods combined | Maximum flexibility | Complex routing |

You've chosen **URL versioning**, which is the most common and easiest for clients to use.

---

## Best Practices

✅ **DO:**
- Version at the URL level for public APIs
- Support multiple versions simultaneously during deprecation period
- Document breaking changes between versions
- Use `MapToApiVersion` to explicitly map methods to versions
- Enable `ReportApiVersions` for client awareness

❌ **DON'T:**
- Remove old versions abruptly without deprecation period
- Mix versioning strategies (stick to URL-based)
- Forget to update Swagger config when adding versions
- Use version numbers in method names (use `MapToApiVersion` instead)

---

## Deprecation Strategy

When deprecating a version:

1. **Add deprecation header** in the method:
```csharp
[MapToApiVersion("1.0")]
[Obsolete("Use v2.0 instead")]
[HttpGet]
public IEnumerable<WeatherForecast> GetV1() { ... }
```

2. **Document timeline** - Give clients 6+ months to migrate

3. **Remove after period** - Only remove when no clients use it

---

## Summary of Your Setup

| Component | Configuration |
|-----------|---------------|
| **Versioning Type** | URL Segment (`/api/v{version}/...`) |
| **Supported Versions** | 1.0, 2.0 |
| **Default Version** | 1.0 |
| **Version Reader** | `UrlSegmentApiVersionReader` |
| **Swagger Support** | ✅ Yes (both v1 & v2 docs) |
| **Version Reporting** | ✅ Yes (headers enabled) |

Your API is production-ready for URL-level versioning! 🚀
