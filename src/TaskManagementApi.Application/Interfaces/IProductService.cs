using TaskManagementApi.Application.DTOs.Products;

namespace TaskManagementApi.Application.Interfaces
{
    public interface IProductService
    {
        Task<ProductResponse> CreateProductAsync(int projectId, CreateProductRequest request);
        Task<IEnumerable<ProductResponse>> GetProductsByProjectIdAsync(int projectId);
        Task<ProductResponse?> GetProductByIdAsync(int id);
        Task UpdateProductAsync(int id, UpdateProductRequest request);
        Task SoftDeleteProductAsync(int id);
    }
}
