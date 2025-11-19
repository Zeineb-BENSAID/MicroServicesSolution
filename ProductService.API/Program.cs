using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions;
using Microsoft.OpenApi.Models;
using ProductService.Application.Interfaces;
using ProductService.Application.Services;
using ProductService.Infrastructure.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using ProductService.Domain.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//***********************************************************
builder.Services.AddDbContext<ProductDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped(typeof(IGenericRepository<>),typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProductService,ProductServices>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//***********************************************************
builder.Services.AddControllers().AddJsonOptions(options => { // Use camelCase for property names options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

    // Ignore null values
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

    // Example: Add a custom converter
    // options.JsonSerializerOptions.Converters.Add(new YourCustomConverter());

    // Example: Allow case-insensitive property names
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddApiVersioning(options =>
{
    // Sets the default API version to 1.0
    options.DefaultApiVersion = new ApiVersion(1, 0);

    // Assumes the default version when the client does not specify one
    options.AssumeDefaultVersionWhenUnspecified = true;

    // Adds a response header to indicate supported API versions
    options.ReportApiVersions = true;
}).AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
builder.Services.AddSwaggerGen(c =>
{
    // ⚠️ Critical: Resolve IApiVersionDescriptionProvider
    var provider = builder.Services.BuildServiceProvider()
        .GetRequiredService<IApiVersionDescriptionProvider>();

    // Generate one Swagger doc per API version
    foreach (var description in provider.ApiVersionDescriptions)
    {
        c.SwaggerDoc(
            description.GroupName, // e.g., "v1.0", "v2.0"
            new OpenApiInfo
            {
                Title = $"Product API {description.GroupName}",
                Version = description.ApiVersion.ToString(),
                Description = $"Product API - Version {description.ApiVersion}"
            });
    }

    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    // Optional: c.EnableAnnotations(); if using [SwaggerOperation] etc.
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    //app.UseSwaggerUI();
    app.UseSwaggerUI(c =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        // Add each version as a separate Swagger endpoint
        foreach (var description in provider.ApiVersionDescriptions)
        {
            c.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant()  // e.g., "V1.0", "V2.0"
            );
        }

        c.RoutePrefix ="swagger"; // optional: Swagger at /
    });
}


// Centralized error handling middleware

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
