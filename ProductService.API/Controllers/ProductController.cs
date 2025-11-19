using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProductService.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _service;

        public ProductController(IProductService service) => _service = service;

        // v1: old behavior
        [HttpGet, MapToApiVersion("1.0")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsV1(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filter = null)
        {
            var products = await _service.GetAllAsync(pageNumber, pageSize, filter);
            return Ok(products);
        }

        // v2: new behavior (example: different default page size or extra metadata)
        [HttpGet, MapToApiVersion("2.0")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsV2(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            [FromQuery] string? filter = null)
        {
            var products = await _service.GetAllAsync(pageNumber, pageSize, filter);
            // return same shape as v1 for now; change as needed for v2-specific behaviour
            return Ok(products);
        }

        [HttpGet("{id}"), MapToApiVersion("1.0"), MapToApiVersion("2.0")]
        public async Task<ActionResult<ProductDto>> Get(Guid id)
        {
            var product = await _service.GetByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> Post(ProductDto productDto)
        {
            var createdProduct = await _service.CreateAsync(productDto);

            // include the requested API version so Created Location points to correct versioned route
            var apiVersion = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
            return CreatedAtAction(nameof(Get), new { version = apiVersion, id = createdProduct.Id }, createdProduct);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, ProductDto productDto)
        {
            await _service.UpdateAsync(id, productDto);
            return NoContent();
        }

        [HttpDelete("{id}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
        [HttpGet("throw"), MapToApiVersion("1.0")]
        public IActionResult ThrowError()
        {
            throw new Exception("Test exception for global error handler.");
        }
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
    }
}