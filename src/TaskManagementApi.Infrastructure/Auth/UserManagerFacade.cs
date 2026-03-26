using Microsoft.AspNetCore.Identity;
using TaskManagementApi.Domain.Entities;
using TaskManagementApi.Web.Controllers;
using Microsoft.EntityFrameworkCore;

namespace TaskManagementApi.Infrastructure.Auth
{
    public class UserManagerFacade : IUserManagerFacade
    {
        private readonly UserManager<User> _userManager;

        public UserManagerFacade(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<User?> FindByProviderIdAsync(string providerId)
        {
            return await _userManager.Users.FirstOrDefaultAsync(u => u.ProviderId == providerId);
        }
    }
}
