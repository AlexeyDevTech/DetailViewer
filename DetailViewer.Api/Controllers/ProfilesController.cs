using DetailViewer.Api.Data;
using DetailViewer.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DetailViewer.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfilesController : BaseController<Profile>
    {
        public ProfilesController(ApplicationDbContext context, ILogger<ProfilesController> logger) : base(context, logger)
        {
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Profile>>> GetAllProfiles()
        {
            _logger.LogInformation("Getting all profiles");
            return await _context.Profiles.ToListAsync();
        }
    }
}
