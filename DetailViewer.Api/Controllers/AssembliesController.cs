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
    public class AssembliesController : BaseController<Assembly>
    {
        public AssembliesController(ApplicationDbContext context, ILogger<AssembliesController> logger) 
            : base(context, logger)
        {
        }

        [HttpGet("{id}/parents")]
        public async Task<ActionResult<IEnumerable<Assembly>>> GetParentAssemblies(int id)
        {
            _logger.LogInformation($"Getting parent assemblies for assembly with id {id}");
            return await _context.AssemblyParents
                .Where(ap => ap.ChildAssemblyId == id)
                .Select(ap => ap.ParentAssembly)
                .Include(a => a.EskdNumber)
                .ThenInclude(e => e.ClassNumber)
                .ToListAsync();
        }

        [HttpGet("{id}/products")]
        public async Task<ActionResult<IEnumerable<Product>>> GetRelatedProducts(int id)
        {
            _logger.LogInformation($"Getting related products for assembly with id {id}");
            return await _context.ProductAssemblies
                .Where(pa => pa.AssemblyId == id)
                .Select(pa => pa.Product)
                .Include(p => p.EskdNumber)
                .ThenInclude(e => e.ClassNumber)
                .ToListAsync();
        }

        [HttpPut("{id}/parents")]
        public async Task<IActionResult> UpdateParentAssemblies(int id, List<int> parentIds)
        {
            _logger.LogInformation($"Updating parent assemblies for assembly with id {id}");
            var assembly = await _context.Assemblies.FindAsync(id);
            if (assembly == null)
            {
                _logger.LogWarning($"Assembly with id {id} not found");
                return NotFound();
            }

            var existingParents = await _context.AssemblyParents.Where(ap => ap.ChildAssemblyId == id).ToListAsync();
            _context.AssemblyParents.RemoveRange(existingParents);

            if (parentIds != null)
            {
                foreach (var parentId in parentIds)
                {
                    _context.AssemblyParents.Add(new AssemblyParent { ChildAssemblyId = id, ParentAssemblyId = parentId });
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}/products")]
        public async Task<IActionResult> UpdateRelatedProducts(int id, List<int> productIds)
        {
            _logger.LogInformation($"Updating related products for assembly with id {id}");
            var assembly = await _context.Assemblies.FindAsync(id);
            if (assembly == null)
            {
                _logger.LogWarning($"Assembly with id {id} not found");
                return NotFound();
            }

            var existingProducts = await _context.ProductAssemblies.Where(pa => pa.AssemblyId == id).ToListAsync();
            _context.ProductAssemblies.RemoveRange(existingProducts);

            if (productIds != null)
            {
                foreach (var productId in productIds)
                {
                    _context.ProductAssemblies.Add(new ProductAssembly { AssemblyId = id, ProductId = productId });
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}