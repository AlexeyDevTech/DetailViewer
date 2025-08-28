using DetailViewer.Api.Data;
using DetailViewer.Api.DTOs;
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
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ProductsController"/>.
        /// </summary>
        /// <param name="context">Контекст базы данных приложения.</param>
        /// <param name="logger">Логгер для контроллера.</param>
        public ProductsController(ApplicationDbContext context, ILogger<ProductsController> logger) 
            : base(context, logger)
        {
        }

        /// <summary>
        /// Создает новый продукт с связанным номером ЕСКД и родительскими сборками.
        /// </summary>
        /// <param name="dto">DTO, содержащий данные для создания продукта.</param>
        /// <returns>Созданный продукт.</returns>
        [HttpPost]
        public async Task<ActionResult<Product>> Post(ProductCreateDto dto)
        {
            _logger.LogInformation("Creating new product with complex payload");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ESKDNumbers.Add(dto.EskdNumber);
                await _context.SaveChangesAsync();

                dto.Product.EskdNumberId = dto.EskdNumber.Id;
                _context.Products.Add(dto.Product);
                await _context.SaveChangesAsync();

                if (dto.ParentAssemblyIds != null && dto.ParentAssemblyIds.Any())
                {
                    foreach (var parentId in dto.ParentAssemblyIds)
                    {
                        _context.ProductAssemblies.Add(new ProductAssembly { ProductId = dto.Product.Id, AssemblyId = parentId });
                    }
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return CreatedAtAction(nameof(Get), new { id = dto.Product.Id }, dto.Product);
            }
            catch (System.Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating product with complex payload");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Получает родительские сборки для данного продукта.
        /// </summary>
        /// <param name="id">Идентификатор продукта.</param>
        /// <returns>Список родительских сборок.</returns>
        [HttpGet("{id}/parents")]
        public async Task<ActionResult<IEnumerable<Assembly>>> GetParentAssemblies(int id)
        {
            _logger.LogInformation($"Getting parent assemblies for product with id {id}");
            return await _context.ProductAssemblies
                .Where(pa => pa.ProductId == id)
                .Include(pa => pa.Assembly.EskdNumber.ClassNumber)
                .Select(pa => pa.Assembly)
                .ToListAsync();
        }

        /// <summary>
        /// Обновляет родительские сборки для данного продукта.
        /// </summary>
        /// <param name="id">Идентификатор продукта.</param>
        /// <param name="parentIds">Список идентификаторов родительских сборок.</param>
        /// <returns>NoContent в случае успеха, NotFound, если продукт не существует.</returns>
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

        /// <summary>
        /// Преобразует продукт в сборку, перенося связанный номер ЕСКД и родительско-дочерние отношения.
        /// </summary>
        /// <param name="id">Идентификатор продукта для преобразования.</param>
        /// <param name="childProductIds">Список идентификаторов продуктов, которые будут связаны как дочерние с новой сборкой.</param>
        /// <returns>Вновь созданная сборка.</returns>
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