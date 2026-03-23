using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Application.DTOs.Products;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Domain.Entities;

namespace TaskManagementApi.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IRepository<Product> _productRepository;

        public ProductService(IRepository<Product> productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<ProductResponse> CreateProductAsync(int projectId, CreateProductRequest request)
        {
            var product = new Product
            {
                ProjectId = projectId,
                VersionName = request.VersionName,
                Description = request.Description,
                PlannedReleaseDate = request.PlannedReleaseDate,
                ReleaseType = request.ReleaseType,
                Status = request.Status
            };

            await _productRepository.AddAsync(product);

            return MapToResponse(product);
        }

        public async Task<IEnumerable<ProductResponse>> GetProductsByProjectIdAsync(int projectId)
        {
            var products = await _productRepository.Query().Where(p => p.ProjectId == projectId).ToListAsync();
            return products.Select(MapToResponse);
        }

        public async Task<ProductResponse?> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            return product != null ? MapToResponse(product) : null;
        }

        public async Task UpdateProductAsync(int id, UpdateProductRequest request)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return;

            product.VersionName = request.VersionName;
            product.Description = request.Description;
            product.PlannedReleaseDate = request.PlannedReleaseDate;
            product.ReleaseType = request.ReleaseType;
            product.Status = request.Status;

            await _productRepository.UpdateAsync(product);
        }

        public async Task SoftDeleteProductAsync(int id)
        {
            await _productRepository.DeleteAsync(id);
        }

        private ProductResponse MapToResponse(Product product)
        {
            return new ProductResponse
            {
                Id = product.Id,
                ProjectId = product.ProjectId,
                VersionName = product.VersionName,
                Description = product.Description,
                PlannedReleaseDate = product.PlannedReleaseDate,
                ReleaseType = product.ReleaseType,
                Status = product.Status
            };
        }
    }
}
