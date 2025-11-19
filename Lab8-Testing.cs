
# Step 3: Add Unit Testing for ProductService

---

## 1. Setup Test Project

Create a test project, e.g. `ProductService.Tests`.

Install NuGet packages:

```bash
dotnet add ProductService.Tests package xunit
dotnet add ProductService.Tests package xunit.runner.visualstudio
dotnet add ProductService.Tests package Moq
dotnet add ProductService.Tests package Microsoft.EntityFrameworkCore.InMemory
dotnet add ProductService.Tests package FluentAssertions
```

---

## 2. Test the Repository (using InMemory EF Core)

```csharp
public class ProductRepositoryTests
{
    private readonly ProductDbContext _context;
    private readonly ProductRepository _repository;

    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        _context = new ProductDbContext(options);
        _repository = new ProductRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddProduct()
    {
        var product = new Product { Name = "Test Product", Price = 10m, Description = "Test" };

        await _repository.AddAsync(product);

        var saved = await _context.Products.FindAsync(product.Id);
        saved.Should().NotBeNull();
        saved.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnFilteredProducts()
    {
        _context.Products.AddRange(
            new Product { Name = "Apple", Price = 5m, Description = "Fruit" },
            new Product { Name = "Banana", Price = 3m, Description = "Fruit" });
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync(1, 10, "Apple", "name", true);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Apple");
    }
}
```

---

## 3. Test the Service Layer (using Moq)

```csharp
public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repoMock;
    private readonly IMapper _mapper;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _repoMock = new Mock<IProductRepository>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _service = new ProductService(_repoMock.Object, _mapper);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProductDto()
    {
        var product = new Product { Id = 1, Name = "Test", Price = 10m, Description = "Test desc" };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        var result = await _service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Test");
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedProduct()
    {
        var dto = new ProductDto { Name = "New Product", Price = 15m, Description = "Desc" };
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(dto);

        result.Should().NotBeNull();
        result.Name.Should().Be("New Product");
    }
}
```

---

## 4. Test the API Controller (using Moq and TestServer)

```csharp
public class ProductsControllerTests
{
    private readonly Mock<IProductService> _serviceMock;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _serviceMock = new Mock<IProductService>();
        _controller = new ProductsController(_serviceMock.Object);
    }

    [Fact]
    public async Task Get_ReturnsOkResult_WithListOfProducts()
    {
        _serviceMock.Setup(s => s.GetAllAsync(1, 10, null, null, true))
            .ReturnsAsync((new List<ProductDto>
            {
                new ProductDto { Id = 1, Name = "Prod1", Price = 5, Description = "Desc" }
            }, 1));

        var result = await _controller.Get();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var products = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(okResult.Value);
        Assert.Single(products);
    }

    [Fact]
    public async Task Get_ById_ReturnsNotFound_WhenProductNull()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(42)).ReturnsAsync((ProductDto?)null);

        var result = await _controller.Get(42);

        Assert.IsType<NotFoundResult>(result.Result);
    }
}
```

---

## Summary

*Use * *InMemory EF Core** for repository tests.
* Use **Moq** for mocking dependencies in service and controller tests.
* Use **FluentAssertions** for clean, readable assertions.
* Test CRUD operations, filtering, pagination, and error cases.

