# Advanced REST API Structuring in ASP.NET Core

## Global Error handling

In your Program.cs, add centralized error handling middleware:

```csharp
using Microsoft.AspNetCore.Diagnostics;

// ... existing code ...

var app = builder.Build();

// Centralized error handling middleware
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var error = new
        {
            Message = "An unexpected error occurred.",
            Detail = app.Environment.IsDevelopment() ? exceptionHandlerPathFeature?.Error.Message : null
        };
        await context.Response.WriteAsJsonAsync(error);
    });
});

// ... rest of your middleware ...
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
 ```
by default, ASP.NET Core does not provide a custom, centralized way to handle and format unhandled exceptions for your API.

### Test 

Add the following test action to your ProductController:

```csharp
[HttpGet("throw")]
public IActionResult ThrowError()
{
    throw new Exception("Test exception for global error handler.");
}
 ```

https://localhost:7219/api/v1/Product/throw



------------------------------------------------------------------------------------------------------------------------------------------
## Custom error hundling 

The default global handler only returns a generic message for all exceptions.

### Create a custom exception class:
```csharp
// In ProductService.Application/Exceptions/NotEnoughStockException.cs
using System;

namespace ProductService.Application.Exceptions
{
    public class NotEnoughStockException : Exception
    {
        public NotEnoughStockException(string message) : base(message) { }
        
    }
}
```
### Throw the custom exception in your service:
```csharp
// In ProductServices.cs (example)
using ProductService.Application.Exceptions;

public async Task UpdateStockAsync(Guid productId, int quantity)
{
    // ... some logic ...
    if (quantity > availableStock)
    {
        throw new NotEnoughStockException("Not enough stock available for this product.");
    }
    // ... update logic ...
}


 public async Task UpdateStockAsync(Guid productId, int quantity)
     {
         // Retrieve the product from the repository
         var product = await _repository.GetByIdAsync(productId);
         if (product == null)
             throw new KeyNotFoundException("Product not found");

         // Suppose you have a Stock property in your Product entity
         // If not, you need to add it to the Product class and database
         if (quantity > product.Stock)
         {
             throw new NotEnoughStockException("Not enough stock available for this product.");
         }

         // Update the stock
         product.Stock -= quantity;
         await _repository.UpdateAsync(product);
     }
 }
```
### Handle the custom exception in your global error handler:
```csharp
/ In Program.cs, update the error handler:
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        context.Response.ContentType = "application/json";
        if (exception is ProductService.Application.Exceptions.NotEnoughStockException)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new
            {
                Message = exception.Message
            });
        }
        else
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new
            {
                Message = "An unexpected error occurred !!!!!!!!!!!!!!!!!!.",
                Detail = app.Environment.IsDevelopment() ? exception?.Message : null
            });
        }
    });
});
```

In controller 

```csharp
 // POST: api/v1/Product/{id}/updatestock?quantity=5
 [HttpPost("{id:guid}/updatestock")]
 public async Task<IActionResult> UpdateStock(Guid id, [FromQuery] int quantity)
 {
     try
     {
         await _service.UpdateStockAsync(id, quantity);
         return NoContent();
     }
     catch (KeyNotFoundException ex)
     {
         return NotFound(new { Message = ex.Message });
     }
     // NotEnoughStockException will be handled globally by your middleware
     //catch (NotEnoughStockException ex)
     //{
     //    // Return 400 Bad Request with a custom message
     //    return BadRequest(new { Message = ex.Message });
     //}
 }
 ```