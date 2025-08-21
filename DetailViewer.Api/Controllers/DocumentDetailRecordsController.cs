using DetailViewer.Api.Data;
using DetailViewer.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DetailViewer.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentDetailRecordsController : BaseController<DocumentDetailRecord>
    {
        public DocumentDetailRecordsController(ApplicationDbContext context, ILogger<DocumentDetailRecordsController> logger) 
            : base(context, logger)
        {
        }

        [HttpGet("{id}/parents")]
        public async Task<ActionResult<IEnumerable<Assembly>>> GetParentAssemblies(int id)
        {
            _logger.LogInformation($"Getting parent assemblies for document detail record with id {id}");
            return await _context.AssemblyDetails
                .Where(ad => ad.DetailId == id)
                .Select(ad => ad.Assembly)
                .Include(a => a.EskdNumber)
                .ThenInclude(e => e.ClassNumber)
                .ToListAsync();
        }
    }
}