using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TaskManagementApi.Application.DTOs.Auth;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Domain.Entities;

namespace TaskManagementApi.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IRepository<UserProjectRole> _userProjectRoleRepository;
        private readonly UserManager<User> _userManager;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            IRepository<User> userRepository,
            IRepository<Role> roleRepository,
            IRepository<UserProjectRole> userProjectRoleRepository,
            UserManager<User> userManager,
            IOptions<JwtSettings> jwtSettings)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userProjectRoleRepository = userProjectRoleRepository;
            _userManager = userManager;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            // Check if email already exists
            var existing = await _userRepository.Query()
                .FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existing != null)
                throw new InvalidOperationException(
                    "An account with this email already exists");

            // Hash password using BCrypt
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Email = request.Email,
                UserName = request.Email,
                DisplayName = request.DisplayName,
                PasswordHash = passwordHash,
                IsActive = true,
                Provider = "local",
                ProviderId = request.Email,
                CreatedAt = DateTime.UtcNow
            };
            await _userRepository.AddAsync(user);

            return await GenerateAuthResponseAsync(user);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            // Find user by email
            var user = await _userRepository.Query()
                .FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                throw new UnauthorizedAccessException(
                    "Invalid email or password");

            // Check if account is active
            if (!user.IsActive)
                throw new UnauthorizedAccessException(
                    "This account has been deactivated");

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException(
                    "Invalid email or password");

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            return await GenerateAuthResponseAsync(user);
        }

        public async Task<AuthResponse> RefreshAsync(string refreshToken)
        {
            var user = await _userRepository.Query()
                .Include(u => u.UserProjectRoles)
                .ThenInclude(upr => upr.Role)
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
            if (user == null)
                throw new UnauthorizedAccessException(
                    "Invalid refresh token");
            if (user.RefreshTokenExpiry < DateTime.UtcNow)
                throw new UnauthorizedAccessException(
                    "Refresh token has expired");

            return await GenerateAuthResponseAsync(user);
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var user = await _userRepository.Query()
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
            if (user == null) return;

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _userRepository.UpdateAsync(user);
        }

        public async Task<UserDto> GetCurrentUserAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            return await MapToUserDtoAsync(user);
        }

        private async Task<AuthResponse> GenerateAuthResponseAsync(User user)
        {
            // Get user roles from projects
            var projectRoles = await _userProjectRoleRepository.Query()
                .Where(upr => upr.UserId == user.Id)
                .Select(upr => upr.Role.Name!)
                .ToListAsync();

            // Get global identity roles
            var identityRoles = await _userManager.GetRolesAsync(user);

            var roles = projectRoles.Concat(identityRoles).Distinct().ToList();

            return await GenerateAuthResponseInternalAsync(user, roles);
        }

        private async Task<AuthResponse> GenerateAuthResponseInternalAsync(User user, List<string> roles)
        {
            var primaryRole = roles.Contains("Admin") ? "Admin"
                : roles.Contains("ProjectManager") ? "ProjectManager"
                : roles.FirstOrDefault() ?? "Developer";

            // Generate access token with roles
            var accessToken = GenerateAccessToken(user, roles, primaryRole);

            // Generate refresh token
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow
                .AddDays(_jwtSettings.RefreshTokenExpiryDays);
            await _userRepository.UpdateAsync(user);

            return new AuthResponse
            {
                Token = accessToken,
                UserId = user.Id,
                Email = user.Email ?? "",
                DisplayName = user.DisplayName,
                Role = primaryRole,
                Permissions = DerivePermissions(primaryRole),
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = _jwtSettings.AccessTokenExpiryMinutes * 60,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    DisplayName = user.DisplayName,
                    AvatarUrl = user.AvatarUrl,
                    Roles = roles
                }
            };
        }

        private PermissionsDto DerivePermissions(string role)
        {
            if (role == "Admin")
            {
                return new PermissionsDto
                {
                    CanCreateProject = true,
                    CanEditProject = true,
                    CanDeleteProject = true,
                    CanAssignTickets = true,
                    CanManageUsers = true,
                    CanViewReports = true,
                    CanViewCosting = true,
                    CanApproveClientBugs = true
                };
            }

            if (role == "ProjectManager")
            {
                return new PermissionsDto
                {
                    CanCreateProject = true,
                    CanEditProject = true,
                    CanDeleteProject = false,
                    CanAssignTickets = true,
                    CanManageUsers = false,
                    CanViewReports = true,
                    CanViewCosting = true,
                    CanApproveClientBugs = true
                };
            }

            // Developer, QAEngineer, BusinessAnalyst
            return new PermissionsDto
            {
                CanCreateProject = true, // for tickets
                CanEditProject = false,
                CanDeleteProject = false,
                CanAssignTickets = false,
                CanManageUsers = false,
                CanViewReports = false,
                CanViewCosting = false,
                CanApproveClientBugs = false
            };
        }

        private string GenerateAccessToken(User user, IEnumerable<string> roles, string primaryRole)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Name, user.DisplayName),
                new Claim(ClaimTypes.Role, primaryRole),
                new Claim("sub", user.Id.ToString()),
                new Claim("email", user.Email ?? ""),
                new Claim("name", user.DisplayName),
            };

            foreach (var role in roles)
            {
                if (role != primaryRole)
                    claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    _jwtSettings.AccessTokenExpiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        private async Task<UserDto> MapToUserDtoAsync(User user)
        {
            // Get user roles from projects
            var projectRoles = await _userProjectRoleRepository.Query()
                .Where(upr => upr.UserId == user.Id)
                .Select(upr => upr.Role.Name!)
                .ToListAsync();

            // Get global identity roles
            var identityRoles = await _userManager.GetRolesAsync(user);

            var roles = projectRoles.Concat(identityRoles).Distinct().ToList();

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? "",
                DisplayName = user.DisplayName,
                AvatarUrl = user.AvatarUrl,
                Roles = roles
            };
        }
    }
}
