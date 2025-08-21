using DetailViewer.Api.Data;
using DetailViewer.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DetailViewer.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassifiersController : BaseController<Classifier>
    {
        public ClassifiersController(ApplicationDbContext context, ILogger<ClassifiersController> logger) 
            : base(context, logger)
        {
        }
    }
}