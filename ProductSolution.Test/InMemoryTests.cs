using Xunit;
using Moq;
using AutoMapper;
using ProductService.Application.Services;
using ProductService.Application.DTOs;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductService.Test
{
    public class InMemoryTests
    {
        private readonly ProductServices _service;
        private readonly ProductDbContext _context;
        private readonly IMapper _mapper;

        public InMemoryTests()
        {
            var options = new DbContextOptionsBuilder<ProductDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ProductDbContext(options);

            // You can use a real mapper or a mock, depending on your needs
            var mapperMock = new Mock<IMapper>();
            _mapper = mapperMock.Object;

            var realRepository = new GenericRepository<Product>(_context);
            _service = new ProductServices(realRepository, _mapper);
        }

        [Fact]
        public async Task CreateAndGetById_WorksWithInMemory()
        {
            // Arrange
            var product = new Product { Id = Guid.NewGuid(), Name = "Test", Description = "Desc" };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Act
            var repo = new GenericRepository<Product>(_context);
            var found = await repo.GetByIdAsync(product.Id);

            // Assert
            Assert.NotNull(found);
            Assert.Equal("Test", found.Name);
        }

        // Add more tests as needed, using the real repository and context
    }
}
