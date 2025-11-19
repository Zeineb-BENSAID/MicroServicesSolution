# Lab: Build an API Gateway with YARP for Microservices (.NET 8)

---

## Step 1: Create the API Gateway project

```bash
dotnet new web - n ApiGateway
cd ApiGateway
```

---

## Step 2: Add YARP NuGet package

```bash
dotnet add package Microsoft.ReverseProxy
```

---

## Step 3: Configure YARP in `Program.cs`

Replace the contents of `Program.cs` with:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add YARP reverse proxy services
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Map reverse proxy routes
app.MapReverseProxy();

app.Run();
```

---

## Step 4: Setup `appsettings.json` to define routes and clusters

Create or edit `appsettings.json` to include your microservices routing:

```json
{
    "ReverseProxy": {
        "Routes": {
            "productRoute": {
                "ClusterId": "productCluster",
        "Match": {
                    "Path": "/api/products/{**catch-all}"
        }
            },
      "orderRoute": {
                "ClusterId": "orderCluster",
        "Match": {
                    "Path": "/api/orders/{**catch-all}"
        }
            },
      "inventoryRoute": {
                "ClusterId": "inventoryCluster",
        "Match": {
                    "Path": "/api/inventory/{**catch-all}"
        }
            }
        },
    "Clusters": {
            "productCluster": {
                "Destinations": {
                    "productDestination": {
                        "Address": "https://localhost:5001/"
                    }
                }
            },
      "orderCluster": {
                "Destinations": {
                    "orderDestination": {
                        "Address": "https://localhost:5002/"
                    }
                }
            },
      "inventoryCluster": {
                "Destinations": {
                    "inventoryDestination": {
                        "Address": "https://localhost:5003/"
                    }
                }
            }
        }
    }
}
```

> Adjust ports(`5001`, `5002`, `5003`) to the actual running ports of your microservices.

---

## Step 5: Run your microservices and the API Gateway

* Make sure your microservices (`ProductService.API`, `OrderService.API`, `InventoryService.API`) are running on configured ports.
* Run the API Gateway:

```bash
dotnet run
```

---

## Step 6: Test the API Gateway routing

Try accessing via API Gateway:

* `https://localhost:<gateway_port>/api/products`
* `https://localhost:<gateway_port>/api/orders`
* `https://localhost:<gateway_port>/api/inventory`

Requests will be proxied to the corresponding microservice.

---

## Optional: Add load balancing, retries, and other policies

YARP supports advanced features like:

*Load balancing between multiple destinations
* Retry policies on failed requests
* Transforming requests/responses (headers, paths)

You can add these configurations under `Clusters` and `Routes` in `appsettings.json`.

---

