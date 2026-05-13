using Microsoft.AspNetCore.Identity;
using TaskManagementApi.Domain.Entities;
using TaskManagementApi.Application.Interfaces;
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
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.ProviderId == providerId);
            if (user == null)
            {
                // Fallback to email if ProviderId doesn't match
                // Some providers might use email as the unique identifier in different claims
                user = await _userManager.FindByEmailAsync(providerId);
            }
            return user;
        }
    }
}
