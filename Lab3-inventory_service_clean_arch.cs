#  Lab 3 : Création du microservice InventoryService avec architecture Clean Architecture (.NET 8)

## Étape 1️ – Création de la solution `InventoryService` avec structure Clean Architecture

```
InventoryService/
├── InventoryService.sln
├── InventoryService.API/
│   ├── Controllers/
│   │   └── InventoryController.cs
│   ├── Program.cs
│   ├── appsettings.json
│   └── InventoryService.API.csproj
├── InventoryService.Application/
│   ├── Interfaces/
│   │   └── IInventoryService.cs
│   ├── Services/
│   │   └── InventoryService.cs
│   ├── DTOs/
│   │   └── InventoryItemDto.cs
│   └── InventoryService.Application.csproj
├── InventoryService.Domain/
│   ├── Entities/
│   │   └── InventoryItem.cs
│   └── InventoryService.Domain.csproj
├── InventoryService.Infrastructure/
│   ├── Data/
│   │   ├── InventoryDbContext.cs
│   │   └── InventoryRepository.cs
│   ├── Migrations/
│   └── InventoryService.Infrastructure.csproj
└── InventoryService.Tests/
    ├── InventoryService.Tests.csproj
    └── Services/
        └── InventoryServiceTests.cs
```

## Étape 2️ – Configuration InMemory (EF Core)

Dans `InventoryDbContext.cs` :
```csharp
public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

    public DbSet<InventoryItem> InventoryItems { get; set; }
}
```

Dans `Program.cs` :
```csharp
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseInMemoryDatabase("InventoryDb"));
```

## Étape 3️ – Passage à SQL Server

###  Modifier `Program.cs` :
```csharp
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

###  appsettings.json :
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=InventoryDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

###  Générer les migrations :
```bash
dotnet ef migrations add InitialCreate --project ../InventoryService.Infrastructure/ --startup-project ../InventoryService.API/
dotnet ef database update --project ../InventoryService.Infrastructure/ --startup-project ../InventoryService.API/
```


