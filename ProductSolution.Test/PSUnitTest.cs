using Xunit;
using Moq;
using AutoMapper;
using ProductService.Application.Services;
using ProductService.Application.DTOs;
using ProductService.Domain.Entities;
using ProductService.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductService.Test
{
    public class PSUnitTest
    {
        private readonly Mock<IGenericRepository<Product>> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly ProductServices _service;

        public PSUnitTest()
        {
            _repositoryMock = new Mock<IGenericRepository<Product>>();
            _mapperMock = new Mock<IMapper>();
            _service = new ProductServices(_repositoryMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsMappedProducts()
        {
            var products = new List<Product> { new Product { Id = Guid.NewGuid(), Name = "Test", Description = "Desc" } };
            var productDtos = new List<ProductDto> { new ProductDto { Name = "Test", Description = "Desc" } };

            _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(), It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>()))
                .ReturnsAsync(products);
            _mapperMock.Setup(m => m.Map<IEnumerable<ProductDto>>(products)).Returns(productDtos);

            var result = await _service.GetAllAsync(1, 10);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Test", result.First().Name);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsMappedProduct()
        {
            var id = Guid.NewGuid();
            var product = new Product { Id = id, Name = "Test", Description = "Desc" };
            var productDto = new ProductDto { Name = "Test", Description = "Desc" };

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(product);
            _mapperMock.Setup(m => m.Map<ProductDto>(product)).Returns(productDto);

            var result = await _service.GetByIdAsync(id);

            Assert.NotNull(result);
            Assert.Equal("Test", result.Name);
        }

        [Fact]
        public async Task CreateAsync_AddsProductAndReturnsDto()
        {
            var productDto = new ProductDto { Name = "Test", Description = "Desc" };
            var product = new Product { Id = Guid.NewGuid(), Name = "Test", Description = "Desc" };

            _mapperMock.Setup(m => m.Map<Product>(productDto)).Returns(product);
            _repositoryMock.Setup(r => r.AddAsync(product)).Returns(Task.CompletedTask);
            _mapperMock.Setup(m => m.Map<ProductDto>(product)).Returns(productDto);

            var result = await _service.CreateAsync(productDto);

            Assert.NotNull(result);
            Assert.Equal("Test", result.Name);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesExistingProduct()
        {
            var id = Guid.NewGuid();
            var productDto = new ProductDto { Name = "Updated", Description = "Updated Desc" };
            var existingProduct = new Product { Id = id, Name = "Old", Description = "Old Desc" };

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingProduct);
            _mapperMock.Setup(m => m.Map(productDto, existingProduct));
            _repositoryMock.Setup(r => r.UpdateAsync(existingProduct)).Returns(Task.CompletedTask);

            await _service.UpdateAsync(id, productDto);
            _repositoryMock.Verify(r => r.UpdateAsync(existingProduct), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_DeletesExistingProduct()
        {
            var id = Guid.NewGuid();
            var product = new Product { Id = id, Name = "Test", Description = "Desc" };

            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(product);
            _repositoryMock.Setup(r => r.DeleteAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

            await _service.DeleteAsync(id);

            _repositoryMock.Verify(r => r.DeleteAsync(product), Times.Once);
        }
    }
}
