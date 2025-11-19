# Lab 2: Création du microservice OrderService avec architecture Clean Architecture (.NET 8)

## Étape 1️ – Création de la solution `OrderService` avec structure Clean Architecture

```
OrderService/
├── OrderService.sln
├── OrderService.API/
│   ├── Controllers/
│   │   └── OrdersController.cs
│   ├── Program.cs
│   ├── appsettings.json
│   └── OrderService.API.csproj
├── OrderService.Application/
│   ├── Interfaces/
│   │   └── IOrderService.cs
│   ├── Services/
│   │   └── OrderService.cs
│   ├── DTOs/
│   │   └── OrderDto.cs
│   └── OrderService.Application.csproj
├── OrderService.Domain/
│   ├── Entities/
│   │   └── Order.cs
│   └── OrderService.Domain.csproj
├── OrderService.Infrastructure/
│   ├── Data/
│   │   ├── OrderDbContext.cs
│   │   └── OrderRepository.cs
│   ├── Migrations/
│   └── OrderService.Infrastructure.csproj
└── OrderService.Tests/
    ├── OrderService.Tests.csproj
    └── Services/
        └── OrderServiceTests.cs
```

## Étape 2️ – Configurer la base InMemory

Dans `OrderDbContext.cs` :
```csharp
public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    public DbSet<Order> Orders { get; set; }
}
```

Dans `Program.cs` :
```csharp
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseInMemoryDatabase("OrderDb"));
```

## Étape 3️ – Passage à SQL Server

###  Modifier `Program.cs` :
```csharp
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

###  appsettings.json :
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=OrderDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

###  Générer les migrations :
```bash
dotnet ef migrations add InitialCreate --project ../OrderService.Infrastructure/ --startup-project ../OrderService.API/
dotnet ef database update --project ../OrderService.Infrastructure/ --startup-project ../OrderService.API/
```

 Prêt pour Swagger, DTO, Mapping (AutoMapper), services et repository !
