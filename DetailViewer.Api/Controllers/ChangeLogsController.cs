using DetailViewer.Api.Data;
using DetailViewer.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DetailViewer.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChangeLogsController : BaseController<ChangeLog>
    {
        public ChangeLogsController(ApplicationDbContext context, ILogger<ChangeLogsController> logger) : base(context, logger)
        {
        }

        [HttpGet("since/{timestamp}")]
        public async Task<ActionResult<IEnumerable<ChangeLog>>> GetChangesSince(DateTime timestamp)
        {
            _logger.LogInformation($"Getting changes since {timestamp}");
            return await _context.ChangeLogs.Where(cl => cl.Timestamp > timestamp).ToListAsync();
        }
    }
}
