using DetailViewer.Api.Data;
using DetailViewer.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DetailViewer.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConflictLogsController : BaseController<ConflictLog>
    {
        public ConflictLogsController(ApplicationDbContext context, ILogger<ConflictLogsController> logger) : base(context, logger)
        {
            _logger.LogInformation("ConflictLogsController created");
        }
    }
}
