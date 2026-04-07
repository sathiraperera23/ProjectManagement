using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Infrastructure.Persistence;

namespace TaskManagementApi.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SystemController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SystemController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("db-check")]
        public async Task<IActionResult> CheckDb()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                var migrations = await _context.Database.GetAppliedMigrationsAsync();

                return Ok(new
                {
                    Connected = canConnect,
                    AppliedMigrations = migrations,
                    Time = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
