using System;

namespace DevTools.Instructions
{
    /// <summary>
    /// Provides instructions for generating a unified global solution file (AllServices.sln)
    /// that includes multiple microservice projects.
    /// https://www.youtube.com/watch?v=CqCDOosvZIk
    /// </summary>
    public static class AllServicesSlnInstructions
    {
        public static string GetInstructions()
        {
            return @"
==============================================
✅ SETUP: Global Solution (AllServices.sln)
==============================================

🔹 Purpose:
Create a global solution (AllServices.sln) that includes all microservice projects 
(e.g., ProductService, OrderService, InventoryService), including optional API Gateway.

🔹 Why?
- Open all services together in Visual Studio
- Centralized debugging and browsing
- Keep each service modular (with its own solution)

🔹 Folder Structure Assumed:
- /ProductService/
- /OrderService/
- /InventoryService/
- /ApiGateway/ (optional)

🔹 Script 1: Bash (Linux/macOS or Git Bash)

    #!/bin/bash
    cd "$(dirname "$0")"
    dotnet new sln -n AllServices
    services=("ProductService" "OrderService" "InventoryService")
    for service in "${services[@]}"; do
        find "$service" -type f -name "*.csproj" -exec dotnet sln AllServices.sln add {} \;
    done
    if [ -d "ApiGateway" ]; then
        find "ApiGateway" -type f -name "*.csproj" -exec dotnet sln AllServices.sln add {} \;
    fi

🔹 Script 2: PowerShell (Windows)

# Navigate to the root folder
Set - Location - Path $PSScriptRoot

Write - Host "Creating global solution AllServices.sln..."
dotnet new sln - n AllServices

# List of microservices
$services = @("ProductService", "OrderService", "InventoryService")

foreach ($service in $services) {
    Write - Host "Adding projects for $service"
    Get - ChildItem - Path $service - Recurse - Filter *.csproj | ForEach - Object {
        dotnet sln AllServices.sln add $_.FullName
    }
}

# Optionally add the API Gateway
if (Test - Path "ApiGateway") {
    Write - Host "Adding API Gateway"
    Get - ChildItem - Path "ApiGateway" - Recurse - Filter *.csproj | ForEach - Object {
        dotnet sln AllServices.sln add $_.FullName
    }
}

Write - Host "✅ All projects added to AllServices.sln"

🔹 How to Run:

1-Save the script in the root directory of your lab project(e.g., / microservices - lab /)

2-For Bash script(Linux / macOS / Git Bash):

chmod + x setup - allservices.sh
./ setup - allservices.sh

3-For PowerShell script(Windows):
.\setup - allservices.ps1

4-Open the global solution:
code AllServices.sln  # or open in Visual Studio manually

==============================================
";
        }
    }
}


/* or simply
dotnet sln AllServices.sln add ProductService/ProductService.API/ProductService.API.csproj
dotnet sln AllServices.sln add ProductService/ProductService.Application/ProductService.Application.csproj
dotnet sln AllServices.sln add ProductService/ProductService.Domain/ProductService.Domain.csproj
dotnet sln AllServices.sln add ProductService/ProductService.Infrastructure/ProductService.Infrastructure.csproj

dotnet sln AllServices.sln add OrderService/OrderService.API/OrderService.API.csproj
dotnet sln AllServices.sln add OrderService/OrderService.Application/OrderService.Application.csproj
dotnet sln AllServices.sln add OrderService/OrderService.Domain/OrderService.Domain.csproj
dotnet sln AllServices.sln add OrderService/OrderService.Infrastructure/OrderService.Infrastructure.csproj

dotnet sln AllServices.sln add InventoryService/InventoryService.API/InventoryService.API.csproj
dotnet sln AllServices.sln add InventoryService/InventoryService.Application/InventoryService.Application.csproj
dotnet sln AllServices.sln add InventoryService/InventoryService.Domain/InventoryService.Domain.csproj
dotnet sln AllServices.sln add InventoryService/InventoryService.Infrastructure/InventoryService.Infrastructure.csproj

dotnet sln AllServices.sln add ApiGateway/ApiGateway.csproj

*/