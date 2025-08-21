using DetailViewer.Api.Data;
using DetailViewer.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DetailViewer.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductAssembliesController : BaseController<ProductAssembly>
    {
        public ProductAssembliesController(ApplicationDbContext context, ILogger<ProductAssembliesController> logger) : base(context, logger)
        {
            _logger.LogInformation("ProductAssembliesController created");
        }
    }
}
