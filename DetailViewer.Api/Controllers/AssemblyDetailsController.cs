using DetailViewer.Api.Data;
using DetailViewer.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DetailViewer.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssemblyDetailsController : BaseController<AssemblyDetail>
    {
        public AssemblyDetailsController(ApplicationDbContext context, ILogger<AssemblyDetailsController> logger) : base(context, logger)
        {
            _logger.LogInformation("AssemblyDetailsController created");
        }
    }
}
