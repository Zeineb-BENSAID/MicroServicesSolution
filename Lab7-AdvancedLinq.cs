

### Step 2 Detailed: Implement Advanced LINQ, Pagination, Filtering, Sorting & Aggregations

---

#### 1. Update `IProductRepository.cs`:

Add sorting and aggregation method signatures:

```csharp
Task<IEnumerable<Product>> GetAllAsync(int pageNumber, int pageSize, string? filter, string? sortBy, bool ascending);

Task<int> GetCountAsync(string? filter);

Task<Dictionary<string, decimal>> GetTotalPriceGroupedByNameAsync();
```

---

#### 2. Update `ProductRepository.cs`:

```csharp
public async Task<IEnumerable<Product>> GetAllAsync(int pageNumber, int pageSize, string? filter, string? sortBy, bool ascending)
{
    IQueryable<Product> query = _context.Products.AsQueryable();

    // Filtering
    if (!string.IsNullOrEmpty(filter))
    {
        query = query.Where(p => p.Name.Contains(filter) || p.Description.Contains(filter));
    }

    // Sorting
    if (!string.IsNullOrEmpty(sortBy))
    {
        query = sortBy.ToLower() switch
        {
            "name" => ascending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
            "price" => ascending ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price),
            _ => query.OrderBy(p => p.Name)
        };
    }
    else
    {
        query = query.OrderBy(p => p.Name);
    }

    // Pagination
    query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

    return await query.AsNoTracking().ToListAsync();
}

public async Task<int> GetCountAsync(string? filter)
{
    IQueryable<Product> query = _context.Products;

    if (!string.IsNullOrEmpty(filter))
    {
        query = query.Where(p => p.Name.Contains(filter) || p.Description.Contains(filter));
    }

    return await query.CountAsync();
}

public async Task<Dictionary<string, decimal>> GetTotalPriceGroupedByNameAsync()
{
    return await _context.Products
        .GroupBy(p => p.Name)
        .Select(g => new { Name = g.Key, TotalPrice = g.Sum(p => p.Price) })
        .ToDictionaryAsync(k => k.Name, v => v.TotalPrice);
}
```

---

#### 3. Update `IProductService.cs`:

```csharp
Task<(IEnumerable<ProductDto> Products, int TotalCount)> GetAllAsync(
    int pageNumber, int pageSize, string ? filter = null, string ? sortBy = null, bool ascending = true);

Task<Dictionary<string, decimal>> GetTotalPriceGroupedByNameAsync();
```

---

#### 4. Update `ProductService.cs`:

```csharp
public async Task<(IEnumerable<ProductDto> Products, int TotalCount)> GetAllAsync(
    int pageNumber, int pageSize, string? filter = null, string? sortBy = null, bool ascending = true)
{
    var products = await _repository.GetAllAsync(pageNumber, pageSize, filter, sortBy, ascending);
    var totalCount = await _repository.GetCountAsync(filter);
    var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);
    return (productDtos, totalCount);
}

public async Task<Dictionary<string, decimal>> GetTotalPriceGroupedByNameAsync()
{
    return await _repository.GetTotalPriceGroupedByNameAsync();
}
```

---

#### 5. Update `ProductsController.cs`:

```csharp
[HttpGet]
public async Task<ActionResult> Get(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? filter = null,
    [FromQuery] string? sortBy = null,
    [FromQuery] bool ascending = true)
{
    var (products, totalCount) = await _service.GetAllAsync(pageNumber, pageSize, filter, sortBy, ascending);

    Response.Headers.Add("X-Total-Count", totalCount.ToString());

    return Ok(products);
}

[HttpGet("aggregations/totalPriceByName")]
public async Task<ActionResult<Dictionary<string, decimal>>> GetTotalPriceGroupedByName()
{
    var result = await _service.GetTotalPriceGroupedByNameAsync();
    return Ok(result);
}
```

---

### Summary:

*You added dynamic filtering, sorting, and pagination.
* You included an endpoint showing aggregation (total price grouped by product name).
*You optimize read queries with `AsNoTracking()`.
*The total count is added in response headers to help clients implement pagination UI.

