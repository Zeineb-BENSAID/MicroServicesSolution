using AutoMapper;
using ProductService.Application.DTOs;
using ProductService.Application.Exceptions;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Domain.Interfaces;
using ProductService.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Services
{
    public class ProductServices : IProductService
    {
        private readonly IGenericRepository<Product> _repository;
        private readonly IMapper _mapper;

        public ProductServices(IGenericRepository<Product> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync(int pageNumber, int pageSize, string? filter = null)
        {
            Expression<Func<Product, bool>>? filterExpression = null;

            if (!string.IsNullOrWhiteSpace(filter))
            {
                filterExpression = p => p.Name.Contains(filter) || p.Description.Contains(filter);
            }

            var products = await _repository.GetAllAsync(
                pageNumber: pageNumber,
                pageSize: pageSize,
                filter: filterExpression,
                orderBy: q => q.OrderBy(p => p.Name)
            );

            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto?> GetByIdAsync(Guid id)
        {
            var product = await _repository.GetByIdAsync(id);
            return product == null ? null : _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> CreateAsync(ProductDto productDto)
        {
            var product = _mapper.Map<Product>(productDto);
            await _repository.AddAsync(product);
            return _mapper.Map<ProductDto>(product);
        }

        public async Task UpdateAsync(Guid id, ProductDto productDto)
        {
            var existingProduct = await _repository.GetByIdAsync(id);
            if (existingProduct == null)
                throw new KeyNotFoundException("Product not found");

            _mapper.Map(productDto, existingProduct);
            await _repository.UpdateAsync(existingProduct);
        }

        public async Task DeleteAsync(Guid id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product == null)
                throw new KeyNotFoundException("Product not found");

            await _repository.DeleteAsync(product);
        }
        //The filterExpression is optional and only built when needed.

        //orderBy uses Name as the default sorting logic.
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

}