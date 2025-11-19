# Lab: Build ProductService Microservice with Clean Architecture in .NET 8

---

// Step 1: Create the solution and projects structure

Create a solution `ProductService.sln` with these projects:

* `ProductService.Domain` (Entities, Value Objects)
* `ProductService.Application` (DTOs, Interfaces, Services)
* `ProductService.Infrastructure` (DbContext, Repository, EF Core migrations)
* `ProductService.API` (ASP.NET Core Web API)
* 'ProductService.Tests' xUnit

    ProductService /
├── ProductService.sln
├── ProductService.API /
│   ├── Controllers /
│   │   └── ProductsController.cs
│   ├── Program.cs
│   ├── appsettings.json
│   └── ProductService.API.csproj
├── ProductService.Application /
│   ├── Interfaces /
│   │   └── IProductService.cs
│   ├── Services /
│   │   └── ProductService.cs
│   ├── DTOs /
│   │   └── ProductDto.cs
│   └── ProductService.Application.csproj
├── ProductService.Domain /
│   ├── Entities /
│   │   └── Product.cs
     ── Interfaces /
    │   └── IGenericRepository.cs  
│   ├── ValueObjects /
│   └── ProductService.Domain.csproj
├── ProductService.Infrastructure /
│   ├── Data /
│   │   ├── ProductDbContext.cs
│   │   └── GenericRepository.cs
│   ├── Migrations /
│   └── ProductService.Infrastructure.csproj
└── ProductService.Tests /
    ├── ProductService.Tests.csproj
    └── Services /
        └── ProductServiceTests.cs


| Project                           | References                                                   | Why It's Referenced                                                                                                       |
| --------------------------------- | ------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------- |
| **ProductService.Domain**         | *None*                                                       | It's the core layer. It contains entities and value objects, independent of others.                                       |
| **ProductService.Application**    | → Domain                                                     | Uses domain entities and interfaces (`Product`, etc.) in services and DTO mapping.                                        |
| **ProductService.Infrastructure** | → Domain → Application                                       | Implements repositories using domain models.  Implements interfaces from Application.                                     |
| **ProductService.API**            | → Application → Infrastructure                               | Controllers call services from Application.  Registers DB context and DI from Infrastructure.                             |
| **ProductService.Tests**          | → Application → Domain → Infrastructure *(optional)*         | Tests services from Application. <br> May test domain logic. Infrastructure only if doing integration or repo tests.      |



// Step 2: Define the Product entity (Domain layer)

//Create `Product.cs` in `ProductService.Domain / Entities`:


public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
}
//`IGenericRepository.cs`:
public interface IGenericRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);

    Task<T?> GetByIdAsync(Guid id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}


// Step 3: Implement DbContext and Repository (Infrastructure layer)

//`ProductDbContext.cs`:


public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }
    /*or 
     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Data Source=(localdb)\mssqllocaldb;
        Initial Catalog=ProductDB;Integrated Security=true");
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseLazyLoadingProxies(); 

   */

    public DbSet<Product> Products { get; set; }
}


//`GenericRepository.cs`:


public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly ProductDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(ProductDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
    {
        IQueryable<T> query = _dbSet.AsNoTracking();

        if (filter != null)
            query = query.Where(filter);

        if (orderBy != null)
            query = orderBy(query);

        query = query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        return await query.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }
}



// Step 4: Define DTOs and Service interfaces (Application layer)

//Create `ProductDto.cs` in `ProductService.Application / DTOs`:


public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
}


//Create interface `IProductService.cs` in `ProductService.Application / Interfaces`:

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync(int pageNumber, int pageSize, string? filter = null);
    Task<ProductDto?> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(ProductDto productDto);
    Task UpdateAsync(int id, ProductDto productDto);
    Task DeleteAsync(int id);
}



// Step 5: Implement ProductService (Application layer)

//`ProductService.cs`:


public class ProductService : IProductService
{
    private readonly IGenericRepository<Product> _repository;
    private readonly IMapper _mapper;

    public ProductService(IGenericRepository<Product> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync(int pageNumber, int pageSize, string? filter = null)
    {
        Expression<Func<Product, bool>>? filterExpression = null;

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filterExpression = p => p.Name.Contains(filter) || p.Description.Contains(filter);
        }

        var products = await _repository.GetAllAsync(
            pageNumber: pageNumber,
            pageSize: pageSize,
            filter: filterExpression,
            orderBy: q => q.OrderBy(p => p.Name)
        );

        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        return product == null ? null : _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> CreateAsync(ProductDto productDto)
    {
        var product = _mapper.Map<Product>(productDto);
        await _repository.AddAsync(product);
        return _mapper.Map<ProductDto>(product);
    }

    public async Task UpdateAsync(int id, ProductDto productDto)
    {
        var existingProduct = await _repository.GetByIdAsync(id);
        if (existingProduct == null)
            throw new KeyNotFoundException("Product not found");

        _mapper.Map(productDto, existingProduct);
        await _repository.UpdateAsync(existingProduct);
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null)
            throw new KeyNotFoundException("Product not found");

        await _repository.DeleteAsync(product);
    }
    //The filterExpression is optional and only built when needed.

    //orderBy uses Name as the default sorting logic.
}



// Step 6: Configure AutoMapper profiles

//`MappingProfile.cs` in Application:


public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>().ReverseMap();
    }
}


// Step 7: Setup API Controller (API layer)

//`ProductsController.cs`:

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> Get(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? filter = null)
    {
        var products = await _service.GetAllAsync(pageNumber, pageSize, filter);
        return Ok(products);
    }

    [HttpGet("{id}")]
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
        return CreatedAtAction(nameof(Get), new { id = createdProduct.Id }, createdProduct);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(Guid id, ProductDto productDto)
    {
        await _service.UpdateAsync(id, productDto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}


// Step 8: Configure Program.cs and dependency injection

var builder = WebApplication.CreateBuilder(args);

// Add services

/*builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseInMemoryDatabase("ProductDb"));*/
builder.Services.AddDbContext<ProductDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

uilder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();


// Step 9: Add connection string in `appsettings.json`

//json
{
    "ConnectionStrings": {
        "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ProductDb;Trusted_Connection=True;TrustServerCertificate=True;"
    }
}
//

// Step 10: Create migration and update database
//Installer EF Core Tools - design
Run in terminal:

//bash
dotnet ef migrations add InitialCreate --project ProductService.Infrastructure --startup-project ProductService.API
dotnet ef database update --project ProductService.Infrastructure --startup-project ProductService.API
//

