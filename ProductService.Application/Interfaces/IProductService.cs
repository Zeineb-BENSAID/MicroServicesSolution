using ProductService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync(int pageNumber, int pageSize, string? filter = null);
        Task<ProductDto?> GetByIdAsync(Guid id);
        Task<ProductDto> CreateAsync(ProductDto productDto);
        Task UpdateAsync(Guid id, ProductDto productDto);
        Task DeleteAsync(Guid id);
        Task UpdateStockAsync(Guid productId, int quantity);
    }
}
