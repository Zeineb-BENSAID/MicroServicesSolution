# Advanced REST API Structuring in ASP.NET Core

## API Validation

To integrate validation in your ASP.NET Core Web API, follow these steps:

### 1. Add validation attributes to your DTOs

Decorate properties in your `ProductDto` class with data annotation attributes from `System.ComponentModel.DataAnnotations`:

```csharp
using System.ComponentModel.DataAnnotations;

public class ProductDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name can't be longer than 100 characters.")]
    public string Name { get; set; }

    [Range(0.01, 10000, ErrorMessage = "Price must be between 0.01 and 10,000.")]
    public decimal Price { get; set; }

    [StringLength(500, ErrorMessage = "Description can't be longer than 500 characters.")]
    public string Description { get; set; }
}
```
## 2. Ensure validation is triggered in your controllers

Make sure your controller has the `[ApiController]` attribute. This attribute enables automatic model validation. If the model is invalid, ASP.NET Core will return a **400 Bad Request** with validation errors, so you don’t need to check `ModelState.IsValid` manually.

## 3. Remove manual ModelState checks (If needed)

With `[ApiController]`, you can remove lines like:

```csharp
if (!ModelState.IsValid)
    return BadRequest(ModelState);
```
###   4. Test your API
Send invalid data to your endpoints. The API will automatically return validation errors in the response.






