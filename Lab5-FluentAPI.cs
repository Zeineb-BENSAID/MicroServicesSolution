# Step 11: Add Validation with FluentValidation

---

### 1. Install FluentValidation NuGet packages

Open a terminal at the root of your solution and run:

```bash
dotnet add ProductService.API package FluentValidation.AspNetCore
dotnet add ProductService.Application package FluentValidation
```

* `FluentValidation.AspNetCore` adds integration with ASP.NET Core MVC (for automatic validation in controllers).
* `FluentValidation` package contains core validation APIs used in your Application layer.

---

### 2. Create a Validator class for `ProductDto`

Create a new folder `Validators` inside `ProductService.Application`.

Create a file `ProductDtoValidator.cs`:

```csharp
using FluentValidation;

public class ProductDtoValidator : AbstractValidator<ProductDto>
{
    public ProductDtoValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(100).WithMessage("Name max length is 100 characters");

        RuleFor(p => p.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero");

        RuleFor(p => p.Description)
            .MaximumLength(500).WithMessage("Description max length is 500 characters");
    }
}
```

*This class defines rules for validating properties of `ProductDto`.
*You can extend it with more complex validation logic if needed.

-- -

### 3. Register FluentValidation in `Program.cs` of ProductService.API

In your `Program.cs`, replace or update the service registration for controllers to include FluentValidation integration:

```csharp
builder.Services.AddControllers()
    .AddFluentValidation(fv =>
        fv.RegisterValidatorsFromAssemblyContaining<ProductDtoValidator>());
```

This instructs ASP.NET Core to automatically validate incoming models using all validators found in the assembly containing `ProductDtoValidator`.

---

### 4. Handle validation errors automatically (optional)

By default, FluentValidation integrates with the ModelState validation pipeline in ASP.NET Core and returns a **400 Bad Request** with validation errors if the model is invalid.

You can customize the response format or handling in middleware if needed, but the default behavior is usually sufficient.

---

### 5. Test validation

Try sending a POST or PUT request to your `ProductsController` with invalid data, for example:

```json
{
    "name": "",
  "price": -5,
  "description": "This is a valid description but name and price are invalid."
}
```

The API should respond with validation errors:

```json
{
    "errors": {
        "Name": ["Product name is required"],
    "Price": ["Price must be greater than zero"]
  }
}
```
