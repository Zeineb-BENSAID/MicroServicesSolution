xUnit testing (AAA) 


---

# NUnit vs xUnit — Differences Explained 

## 1. **Philosophy & Design**

### **NUnit**

* Mature and widely used.
* Traditional unit testing style.
* Attribute-based lifecycle (`[SetUp]`, `[TearDown]`).

### **xUnit**

* Created by the original authors of NUnit.
* More modern and opinionated.
* Fewer attributes, encourages clean test structure.

---

## 2. **Attributes Comparison**

| Feature              | NUnit            | xUnit                       |
| -------------------- | ---------------- | --------------------------- |
| Simple test          | `[Test]`         | `[Fact]`                    |
| Param test           | `[TestCase]`     | `[Theory]` + `[InlineData]` |
| Setup per test       | `[SetUp]`        | Constructor                 |
| Cleanup per test     | `[TearDown]`     | `Dispose()`                 |
| Global setup         | `[OneTimeSetUp]` | Collection fixtures         |
| Test class attribute | `[TestFixture]`  | None needed                 |

---

## 3. **Setup & Cleanup Examples**

### **NUnit**

```csharp
[TestFixture]
public class CalculatorTests
{
    [SetUp]
    public void Setup()
    {
        // Runs before each test
    }

    [TearDown]
    public void Cleanup()
    {
        // Runs after each test
    }
}
```

### **xUnit**

```csharp
public class CalculatorTests : IDisposable
{
    public CalculatorTests()
    {
        // Setup code
    }

    public void Dispose()
    {
        // Cleanup code
    }
}
```

---

## 4. **Parameterized Tests**

### **NUnit**

```csharp
[TestCase(2, 2, 4)]
[TestCase(1, 3, 4)]
public void AddTest(int a, int b, int expected)
{
    Assert.AreEqual(expected, a + b);
}
```

### **xUnit**

```csharp
[Theory]
[InlineData(2, 2, 4)]
[InlineData(1, 3, 4)]
public void AddTest(int a, int b, int expected)
{
    Assert.Equal(expected, a + b);
}
```
**[Fact]**  
  A simple test without parameters. Use when the test should always give the same result.
  ```csharp
  [Fact]
  public void Add_ShouldReturnCorrectSum()
  {
      Assert.Equal(4, Calculator.Add(2, 2));
  }
```

## 5. **Assert Syntax**

### **NUnit**

```csharp
Assert.AreEqual(expected, actual);
Assert.IsTrue(condition);
```

### **xUnit**

```csharp
Assert.Equal(expected, actual);
Assert.True(condition);
```

---

## 6. **Popularity & Use Cases**

| Category                          | NUnit  | xUnit   |
| --------------------------------- | ------ | ------- |
| Legacy/enterprise apps            | ⭐⭐⭐⭐   | ⭐⭐      |
| Modern .NET apps                  | ⭐⭐⭐    | ⭐⭐⭐⭐    |
| Default in ASP.NET Core templates | ❌      | ✔️      |
| Complexity                        | Low    | Medium  |
| Community trend                   | Stable | Growing |

---

## Summary

| Topic                        | NUnit         | xUnit                       |
| ---------------------------- | ------------- | --------------------------- |
| Style                        | Traditional   | Modern & clean              |
| Setup/TearDown               | Attributes    | Constructor + `IDisposable` |
| Test attributes              | Many          | Few                         |
| Parameterized tests          | `[TestCase]`  | `[Theory]`                  |
| Recommended for ASP.NET Core | No            | Yes                         |
| Ease of migration            | Easy to adopt | Slightly stricter           |



# Why use Moq?

We use **Moq** (or any mocking library) in unit testing **to isolate the code under test**.  

## Reasons:

1. **Test in isolation**  
   Test a single class or method without depending on real databases, web services, or other classes.

2. **Control behavior**  
   Make dependencies return specific values or throw exceptions to test all scenarios.

3. **Improve speed**  
   Tests run faster because they don’t hit real external systems.

4. **Increase reliability**  
   Tests aren’t affected by external systems being down or slow.

5. **Focus on logic**  
   Verify that your code behaves correctly, regardless of the dependency’s implementation.

> **Example:**  
> If you’re testing a service that calls a repository, Moq lets you simulate the repository returning a list of objects **without connecting to a real database**.

## AAA Pattern (Arrange-Act-Assert)

**AAA** is a simple pattern to structure unit tests:

1. **Arrange** – set up objects, dependencies, and test data  
2. **Act** – execute the method under test  
3. **Assert** – verify that the result matches the expectation

> Example:
```csharp
var calculator = new Calculator(); // Arrange
int result = calculator.Add(2, 3); // Act
Assert.Equal(5, result);           // Assert
```

# Lab

These tests use **Moq** for mocking dependencies and **xUnit** for assertions. The tests cover the following methods:

- `GetAllAsync`
- `GetByIdAsync`
- `CreateAsync`
- `UpdateAsync`
- `DeleteAsync`

Additional notes:

- **Used Moq** to mock dependencies (`IGenericRepository<Product>` and `IMapper`).
- Each test covers a **specific method** and asserts **expected behavior**.

> **Note:**  
> - Ensure you have the **Moq** and **xUnit** NuGet packages installed.  (+Microsoft.EntityFrameworkCore.InMemory )
> - Adjust **namespaces** or **types** if your actual implementation differs (e.g., if you use a different repository interface).




```csharp
using Xunit;
using Moq;
using AutoMapper;
using ProductService.Application.Services;
using ProductService.Application.DTOs;
using ProductService.Domain.Entities;
using ProductService.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductService.Test
{
    public class PSUnitTest
    {
        private readonly Mock<IGenericRepository<Product>> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly ProductServices _service;

        public PSUnitTest()
        {
            _repositoryMock = new Mock<IGenericRepository<Product>>();
            _mapperMock = new Mock<IMapper>();
            _service = new ProductServices(_repositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsMappedProducts()
        {
            var products = new List<Product> { new Product { Id = Guid.NewGuid(), Name = "Test", Description = "Desc" } };
            var productDtos = new List<ProductDto> { new ProductDto { Name = "Test", Description = "Desc" } };

            _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>()))
                .ReturnsAsync(products);
            _mapperMock.Setup(m => m.Map<IEnumerable<ProductDto>>(products)).Returns(productDtos);

            var result = await _service.GetAllAsync(1, 10);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Test", result.First().Name);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsMappedProduct()
        {
            var id = Guid.NewGuid();
            var product = new Product { Id = id, Name = "Test", Description = "Desc" };
            var productDto = new ProductDto { Name = "Test", Description = "Desc" };

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(product);
            _mapperMock.Setup(m => m.Map<ProductDto>(product)).Returns(productDto);

            var result = await _service.GetByIdAsync(id);

            Assert.NotNull(result);
            Assert.Equal("Test", result.Name);
        }

        [Fact]
        public async Task CreateAsync_AddsProductAndReturnsDto()
        {
            var productDto = new ProductDto { Name = "Test", Description = "Desc" };
            var product = new Product { Id = Guid.NewGuid(), Name = "Test", Description = "Desc" };

            _mapperMock.Setup(m => m.Map<Product>(productDto)).Returns(product);
            _repositoryMock.Setup(r => r.AddAsync(product)).Returns(Task.CompletedTask);
            _mapperMock.Setup(m => m.Map<ProductDto>(product)).Returns(productDto);

            var result = await _service.CreateAsync(productDto);

            Assert.NotNull(result);
            Assert.Equal("Test", result.Name);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesExistingProduct()
        {
            var id = Guid.NewGuid();
            var productDto = new ProductDto { Name = "Updated", Description = "Updated Desc" };
            var existingProduct = new Product { Id = id, Name = "Old", Description = "Old Desc" };

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingProduct);
            _mapperMock.Setup(m => m.Map(productDto, existingProduct));
            _repositoryMock.Setup(r => r.UpdateAsync(existingProduct)).Returns(Task.CompletedTask);

            await _service.UpdateAsync(id, productDto);
            _repositoryMock.Verify(r => r.UpdateAsync(existingProduct), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_DeletesExistingProduct()
        {
            var id = Guid.NewGuid();
            var product = new Product { Id = id, Name = "Test", Description = "Desc" };

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(product);
            _repositoryMock.Setup(r => r.DeleteAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

            await _service.DeleteAsync(id);

            _repositoryMock.Verify(r => r.DeleteAsync(product), Times.Once);
        }
    }
}

```
```csharp

using Xunit;
using Moq;
using AutoMapper;
using ProductService.Application.Services;
using ProductService.Application.DTOs;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductService.Test
{
    public class InMemoryTests
    {
        private readonly ProductServices _service;
        private readonly ProductDbContext _context;
        private readonly IMapper _mapper;

        public InMemoryTests()
        {
            var options = new DbContextOptionsBuilder<ProductDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ProductDbContext(options);

            // You can use a real mapper or a mock, depending on your needs
            var mapperMock = new Mock<IMapper>();
            _mapper = mapperMock.Object;

            var realRepository = new GenericRepository<Product>(_context);
            _service = new ProductServices(realRepository, _mapper);
        }

        [Fact]
        public async Task CreateAndGetById_WorksWithInMemory()
        {
            // Arrange
            var product = new Product { Id = Guid.NewGuid(), Name = "Test", Description = "Desc" };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Act
            var repo = new GenericRepository<Product>(_context);
            var found = await repo.GetByIdAsync(product.Id);

            // Assert
            Assert.NotNull(found);
            Assert.Equal("Test", found.Name);
        }

        // Add more tests as needed, using the real repository and context
    }
}
```

![alt text](image.png)
