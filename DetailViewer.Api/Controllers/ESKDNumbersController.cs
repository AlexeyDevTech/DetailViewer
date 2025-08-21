using DetailViewer.Api.Data;
using DetailViewer.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DetailViewer.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ESKDNumbersController : BaseController<ESKDNumber>
    {
        public ESKDNumbersController(ApplicationDbContext context, ILogger<ESKDNumbersController> logger) : base(context, logger)
        {
            _logger.LogInformation("ESKDNumbersController created");
        }
    }
}
