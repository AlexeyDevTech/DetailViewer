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
    public class ProductsController : BaseController<Product>
    {
        public ProductsController(ApplicationDbContext context, ILogger<ProductsController> logger) : base(context, logger)
        {
        }

        public override async Task<ActionResult<IEnumerable<Product>>> Get()
        {
            _logger.LogInformation("Getting all products");
            return await _context.Products
                .Include(p => p.EskdNumber)
                .ThenInclude(e => e.ClassNumber)
                .ToListAsync();
        }

        public override async Task<ActionResult<Product>> Get(int id)
        {
            _logger.LogInformation($"Getting product with id {id}");
            var entity = await _context.Products
                .Include(p => p.EskdNumber)
                .ThenInclude(e => e.ClassNumber)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (entity == null)
            {
                _logger.LogWarning($"Product with id {id} not found");
                return NotFound();
            }

            return entity;
        }

        [HttpGet("{id}/parents")]
        public async Task<ActionResult<IEnumerable<Assembly>>> GetParentAssemblies(int id)
        {
            _logger.LogInformation($"Getting parent assemblies for product with id {id}");
            return await _context.ProductAssemblies
                .Where(pa => pa.ProductId == id)
                .Select(pa => pa.Assembly)
                .Include(a => a.EskdNumber)
                .ThenInclude(e => e.ClassNumber)
                .ToListAsync();
        }

        [HttpPut("{id}/parents")]
        public async Task<IActionResult> UpdateParentAssemblies(int id, List<int> parentIds)
        {
            _logger.LogInformation($"Updating parent assemblies for product with id {id}");
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                _logger.LogWarning($"Product with id {id} not found");
                return NotFound();
            }

            var existingParents = await _context.ProductAssemblies.Where(pa => pa.ProductId == id).ToListAsync();
            _context.ProductAssemblies.RemoveRange(existingParents);

            if (parentIds != null)
            {
                foreach (var parentId in parentIds)
                {
                    _context.ProductAssemblies.Add(new ProductAssembly { ProductId = id, AssemblyId = parentId });
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/convertToAssembly")]
        public async Task<ActionResult<Assembly>> ConvertToAssembly(int id, List<int> childProductIds)
        {
            _logger.LogInformation($"Converting product with id {id} to assembly");
            using var transaction = await _context.Database.BeginTransactionAsync();

            var productToConvert = await _context.Products
                .Include(p => p.EskdNumber)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (productToConvert == null)
            {
                _logger.LogWarning($"Product with id {id} not found");
                return NotFound();
            }

            var parentAssemblies = await _context.ProductAssemblies
                .Where(pa => pa.ProductId == id)
                .Select(pa => pa.AssemblyId)
                .ToListAsync();

            var newAssembly = new Assembly
            {
                Name = productToConvert.Name,
                EskdNumber = productToConvert.EskdNumber,
            };
            _context.Assemblies.Add(newAssembly);
            await _context.SaveChangesAsync();

            if (parentAssemblies.Any())
            {
                var newParentLinks = parentAssemblies.Select(parentId => new AssemblyParent
                {
                    ParentAssemblyId = parentId,
                    ChildAssemblyId = newAssembly.Id
                }).ToList();
                _context.AssemblyParents.AddRange(newParentLinks);
            }

            if (childProductIds != null && childProductIds.Any())
            {
                var newChildLinks = childProductIds.Select(childId => new ProductAssembly
                {
                    AssemblyId = newAssembly.Id,
                    ProductId = childId
                }).ToList();
                _context.ProductAssemblies.AddRange(newChildLinks);
            }

            await _context.ProductAssemblies.Where(pa => pa.ProductId == id).ExecuteDeleteAsync();
            _context.Products.Remove(productToConvert);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation($"Product with id {id} converted to assembly with id {newAssembly.Id}");
            return CreatedAtAction(nameof(AssembliesController.Get), "Assemblies", new { id = newAssembly.Id }, newAssembly);
        }
    }
}
