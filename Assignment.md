# OrderService — Implementation Assignment  
### Build a microservice identical to ProductService (from scratch)

This assignment evaluates whether you fully understood the architecture, patterns, and concepts used in **ProductService**.  
Your task is to build a **complete OrderService microservice**, following the same structure — **without copying ProductService code**.

---

## 1. Solution & Project Structure

Create the following projects:

- **OrderService.Domain** (class library)  
- **OrderService.Application** (class library)  
- **OrderService.Infrastructure** (class library)  
- **OrderService.API** (Web API)

Your folder structure must mirror ProductService.

---

## 2. Project References

Reference flow must match ProductService:

- Infrastructure → Domain, Application  
- Application → Domain  
- API → Application, Infrastructure  

---

##  3. Domain Layer

Create the following:

### Entities
- `Order`
- `OrderItem`

### Enum
- `OrderStatus`

### Requirements
- Use **Guid** identifiers  
- Navigation property: Order → Items  
- Logic/validation similar to ProductService domain patterns  

---

## 4. Infrastructure Layer

Implement:

- **OrdersDbContext** (EF Core)  
- `DbSet<Order>` and `DbSet<OrderItem>`  
- Fluent configuration for relationships  
- SQL Server configuration  
- Repository pattern:
  - `IOrderRepository`
  - `OrderRepository`
- CRUD operations  
- Migrations  

---

##  5. Application Layer

Implement the same type of artifacts as ProductService:

- DTOs (Order, OrderItem, Create/Update DTOs)  
- AutoMapper profiles  
- Validation (FluentValidation if used in ProductService)  
- Application services 
- Interfaces shared by the layers  

---

##  6. API Layer

Create `OrdersController` with:

### Endpoints
- `GET /api/orders` (with pagination + filtering)
- `GET /api/orders/{id}`
- `POST /api/orders`
- `PUT /api/orders/{id}`
- `DELETE /api/orders/{id}`

### Rules
- Use DTOs only  
- Use Application services (no direct DbContext)  
- Return correct HTTP status codes  
- Enable Swagger  

---

##  7. Configuration

In **OrderService.API**:

- Add SQL Server connection string in `appsettings.json`
- In `Program.cs`, configure:
  - DbContext  
  - Repository  
  - Services  
  - AutoMapper  
  - Validators  
  - Swagger  

Follow ProductService’s conventions exactly.

---

##  8. Migrations & Database

You must:

- Add initial migration  
- Apply migration  
- Ensure database structure is correct (tables, FKs, indexes)  

---

##  9. Unit Tests (XUnit)

Create an XUnit test project for testing OrderService.

### Required tests:
- Creating an order  
- Retrieving an order  
- Updating an order  
- Deleting an order  
- Validation logic (if implemented)  

Use mocks !!!!!!!!!!!!!!!!!!!!!!!!

---

##  10. Quality Expectations

Your OrderService must match ProductService in:

- Clean Architecture  
- Naming conventions  
- Folder structure  
- DTO patterns  
- Repository approach  
- Pagination & filtering  
- Error handling  
- Async programming  
- Unit testing strategy  

---

## 🎓 Purpose

This assignment proves you can:

- Reproduce Clean Architecture  
- Implement EF Core with SQL Server  
- Build a microservice independently  
- Apply DTO + AutoMapper  
- Use repository + service layers  
- Create real CRUD APIs  
- Write meaningful unit tests  

---
