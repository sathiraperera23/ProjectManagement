using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Application.DTOs.Products;
using TaskManagementApi.Application.Interfaces;

namespace TaskManagementApi.Web.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId}/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        public async Task<ActionResult<ProductResponse>> CreateProduct(int projectId, CreateProductRequest request)
        {
            // Requires CREATE_PRODUCT permission
            var response = await _productService.CreateProductAsync(projectId, request);
            return CreatedAtAction(nameof(GetProduct), new { projectId, id = response.Id }, response);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductResponse>>> GetProducts(int projectId)
        {
            var products = await _productService.GetProductsByProjectIdAsync(projectId);
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductResponse>> GetProduct(int projectId, int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int projectId, int id, UpdateProductRequest request)
        {
            // Requires EDIT_PRODUCT permission
            await _productService.UpdateProductAsync(id, request);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int projectId, int id)
        {
            // Requires DELETE_PRODUCT permission
            await _productService.SoftDeleteProductAsync(id);
            return NoContent();
        }
    }
}
