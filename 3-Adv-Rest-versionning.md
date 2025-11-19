# Advanced REST API Structuring in ASP.NET Core
# API Versioning

API Versioning allows you to manage changes to your API over time without breaking existing clients. By versioning your endpoints, you can introduce new features or modify behavior while maintaining backward compatibility.

---

### Step 1: Install the versioning package

First, you need to add the Microsoft-provided package that enables API versioning in ASP.NET Core.

In the terminal, run:

```bash
dotnet add package Microsoft.AspNetCore.Mvc.Versioning
```
This command installs the Microsoft.AspNetCore.Mvc.Versioning NuGet package, which provides all the necessary services and attributes to implement API versioning in your application.

### Step 2: Configure versioning in Program.cs
Next, configure API versioning in your application’s service container:

```csharp
builder.Services.AddApiVersioning(options =>
{
    // Sets the default API version to 1.0
    options.DefaultApiVersion = new ApiVersion(1, 0);

    // Assumes the default version when the client does not specify one
    options.AssumeDefaultVersionWhenUnspecified = true;

    // Adds a response header to indicate supported API versions
    options.ReportApiVersions = true;
});
```
DefaultApiVersion defines the default version your API will use if the client doesn’t specify one.

AssumeDefaultVersionWhenUnspecified ensures older clients can still call endpoints without specifying a version.

ReportApiVersions adds response headers (like api-supported-versions) to inform clients which API versions are available.

### Step 3: Version your controllers

Now, annotate your controllers to indicate which version they support:

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ProductsController : ControllerBase
{
    // Actions here
}
```
Explanation:

[ApiController] marks the class as an API controller.

[Route("api/v{version:apiVersion}/[controller]")] includes the API version in the route URL (e.g., api/v1/Products).

[ApiVersion("1.0")] specifies that this controller responds to version 1.0 of the API.

This setup allows you to easily create new versions later by adding controllers with different [ApiVersion] attributes (e.g., 2.0) while keeping existing clients functional.
----------------------------------------------------------------------------------------------------------------------
# API Versioning Strategies

API versioning allows you to evolve your API without breaking existing clients. There are different approaches depending on whether the behavior of a method changes or stays the same.

---

## 1. Keep the same method, same behavior

If the endpoint logic does not change, you can **reuse the same controller and method** for the new version.

### Example

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[ApiVersion("2.0")] // Supports both v1 and v2
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetProducts()
    {
        // Same logic works for both versions
        return Ok(products);
    }
}
```
### Effect

- Clients calling v1 or v2 get the **same behavior**.

- You don’t need to **duplicate code**.
## 2. Keep the same method, but modify behavior for the new version

If the method needs to behave differently in the new version, you can either **create a new controller class** or **use `MapToApiVersion` on individual methods**.

### Option A – New controller for v2

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("2.0")]
public class ProductsV2Controller : ControllerBase
{
    [HttpGet]
    public IActionResult GetProducts()
    {
        // New behavior for v2
        return Ok(products.Select(p => new { p.Id, p.Name, p.Category }));
    }
}
```
### Option B – Same controller, separate method mapping

```csharp
[HttpGet, MapToApiVersion("1.0")]
public IActionResult GetProductsV1()
{
    // Old behavior
}

[HttpGet, MapToApiVersion("2.0")]
public IActionResult GetProductsV2()
{
    // New behavior
}
```

### Rule of Thumb

- Keep the same method if behavior is identical across versions.

- Create a new method or controller if behavior changes, so old clients are not broken.

- API versioning allows you to safely evolve your API without forcing all clients to update.

### Lab 

In program.cs

``` csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//***********************************************************
builder.Services.AddDbContext<ProductDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//***********************************************************
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IProductServices, ProductServices>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddAutoMapper(config =>
{
    config.AddMaps(typeof(ProductService.Application.Profiles.MappingProfile).Assembly);
});

builder.Services.AddApiVersioning(options =>
{
    // Sets the default API version to 1.0
    options.DefaultApiVersion = new ApiVersion(1, 0);

    // Assumes the default version when the client does not specify one
    options.AssumeDefaultVersionWhenUnspecified = true;

    // Adds a response header to indicate supported API versions
    options.ReportApiVersions = true;
});
var app = builder.Build();

```

In  ProductController 

``` csharp
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Interfaces;
using ProductService.Application.DTOs;

namespace ProductService.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class ProductController : ControllerBase
    {
        private readonly IProductServices _service;

        public ProductController(IProductServices service)
        {
            _service = service;
        }

        // GET: api/v1/Product?pageNumber=1&pageSize=10&filter=abc
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? filter = null)
        {
            var products = await _service.GetAllAsync(pageNumber, pageSize, filter);
            return Ok(products);
        }

        // GET: api/v1/Product/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProductDto>> GetById(Guid id)
        {
            var product = await _service.GetByIdAsync(id);
            if (product == null)
                return NotFound();
            return Ok(product);
        }

        // POST: api/v1/Product
        [HttpPost]
        public async Task<ActionResult<ProductDto>> Create([FromBody] ProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT: api/v1/Product/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _service.UpdateAsync(id, dto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            return NoContent();
        }

        // DELETE: api/v1/Product/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _service.DeleteAsync(id);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
```

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using ProductService.Application.Interfaces;
using ProductService.Domain.Interfaces;
using ProductService.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//***********************************************************
builder.Services.AddDbContext<ProductDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped(typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProductService, ProductService.Application.Services.ProductServices>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//***********************************************************
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddApiVersioning(options =>
{
    // Sets the default API version to 1.0
    options.DefaultApiVersion = new ApiVersion(1, 0);

    // Assumes the default version when the client does not specify one
    options.AssumeDefaultVersionWhenUnspecified = true;

    // Adds a response header to indicate supported API versions
    options.ReportApiVersions = true;
}).AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
builder.Services.AddSwaggerGen(c =>
{
    // ⚠️ Critical: Resolve IApiVersionDescriptionProvider
    var provider = builder.Services.BuildServiceProvider()
        .GetRequiredService<IApiVersionDescriptionProvider>();

    // Generate one Swagger doc per API version
    foreach (var description in provider.ApiVersionDescriptions)
    {
        c.SwaggerDoc(
            description.GroupName, // e.g., "v1.0", "v2.0"
            new OpenApiInfo
            {
                Title = $"Product API {description.GroupName}",
                Version = description.ApiVersion.ToString(),
                Description = $"Product API - Version {description.ApiVersion}"
            });
    }

    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    // Optional: c.EnableAnnotations(); if using [SwaggerOperation] etc.
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    //app.UseSwaggerUI();
    app.UseSwaggerUI(c =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        // Add each version as a separate Swagger endpoint
        foreach (var description in provider.ApiVersionDescriptions)
        {
            c.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant()  // e.g., "V1.0", "V2.0"
            );
        }

        c.RoutePrefix ="swagger"; // optional: Swagger at /
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
************************************************************************
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProductService.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _service;

        public ProductController(IProductService service) => _service = service;

        // v1: old behavior
        [HttpGet, MapToApiVersion("1.0")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsV1(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filter = null)
        {
            var products = await _service.GetAllAsync(pageNumber, pageSize, filter);
            return Ok(products);
        }

        // v2: new behavior (example: different default page size or extra metadata)
        [HttpGet, MapToApiVersion("2.0")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsV2(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            [FromQuery] string? filter = null)
        {
            var products = await _service.GetAllAsync(pageNumber, pageSize, filter);
            // return same shape as v1 for now; change as needed for v2-specific behaviour
            return Ok(products);
        }

        [HttpGet("{id}"), MapToApiVersion("1.0"), MapToApiVersion("2.0")]
        public async Task<ActionResult<ProductDto>> Get(Guid id)
        {
            var product = await _service.GetByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> Post(ProductDto productDto)
        {
            var createdProduct = await _service.CreateAsync(productDto);

            // include the requested API version so Created Location points to correct versioned route
            var apiVersion = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
            return CreatedAtAction(nameof(Get), new { version = apiVersion, id = createdProduct.Id }, createdProduct);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, ProductDto productDto)
        {
            await _service.UpdateAsync(id, productDto);
            return NoContent();
        }

        [HttpDelete("{id}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}