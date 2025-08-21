using DetailViewer.Api.Data;
using DetailViewer.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DetailViewer.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssemblyParentsController : BaseController<AssemblyParent>
    {
        public AssemblyParentsController(ApplicationDbContext context, ILogger<AssemblyParentsController> logger) : base(context, logger)
        {
            _logger.LogInformation("AssemblyParentsController created");
        }
    }
}
