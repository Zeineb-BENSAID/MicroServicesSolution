#  Lab 1: Creating the `ProductService` Microservice with Clean Architecture (.NET 8)

## Step 1️⃣ – Create the `ProductService` solution using Clean Architecture structure

```
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
│   ├── ValueObjects /
│   └── ProductService.Domain.csproj
├── ProductService.Infrastructure /
│   ├── Data /
│   │   ├── ProductDbContext.cs
│   │   └── ProductRepository.cs
│   ├── Migrations /
│   └── ProductService.Infrastructure.csproj
└── ProductService.Tests /
    ├── ProductService.Tests.csproj
    └── Services /
        └── ProductServiceTests.cs


```
| Project                           | References                                                   | Why It's Referenced                                                                                                       |
| --------------------------------- | ------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------- |
| **ProductService.Domain**         | *None*                                                       | It's the core layer. It contains entities and value objects, independent of others.                                       |
| **ProductService.Application**    | → Domain                                                     | Uses domain entities and interfaces (`Product`, etc.) in services and DTO mapping.                                        |
| **ProductService.Infrastructure** | → Domain → Application                                       | Implements repositories using domain models.  Implements interfaces from Application.                                 |
| **ProductService.API**            | → Application → Infrastructure                               | Controllers call services from Application.  Registers DB context and DI from Infrastructure.                         |
| **ProductService.Tests**          | → Application → Domain → Infrastructure *(optional)*         | Tests services from Application. <br> May test domain logic. Infrastructure only if doing integration or repo tests. |

## Step 2️⃣ – Configure InMemory database for quick startup

In `ProductDbContext.cs`:

```csharp
public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

    public DbSet<Product> Products { get; set; }
}
```

In `Program.cs`:

```csharp
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseInMemoryDatabase("ProductDb"));
```

➡️ Run the application and test endpoints using Swagger (`/ api / products`)

## Step 3️⃣ – Switch to SQL Server (replace InMemory)

###  Update `Program.cs`:

```csharp
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

###  Update `appsettings.json`:

```json
"ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ProductDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

###  Create migration and update the database:

```bash
dotnet ef migrations add InitialCreate --project ../ProductService.Infrastructure/ --startup-project ../ProductService.API/
dotnet ef database update --project ../ProductService.Infrastructure/ --startup-project ../ProductService.API/
```

