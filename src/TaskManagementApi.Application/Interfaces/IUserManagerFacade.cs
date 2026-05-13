using TaskManagementApi.Domain.Entities;

namespace TaskManagementApi.Application.Interfaces
{
    public interface IUserManagerFacade
    {
        Task<User?> FindByProviderIdAsync(string providerId);
    }
}
