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
    public class AssembliesController : BaseController<Assembly>
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="AssembliesController"/>.
        /// </summary>
        /// <param name="context">Контекст базы данных приложения.</param>
        /// <param name="logger">Логгер для контроллера.</param>
        public AssembliesController(ApplicationDbContext context, ILogger<AssembliesController> logger) 
            : base(context, logger)
        {
        }

        /// <summary>
        /// Создает новую сборку с связанным номером ЕСКД, родительскими сборками и связанными продуктами.
        /// </summary>
        /// <param name="dto">DTO, содержащий данные для создания сборки.</param>
        /// <returns>Созданная сборка.</returns>
        [HttpPost]
        public async Task<ActionResult<Assembly>> Post(AssemblyCreateDto dto)
        {
            _logger.LogInformation("Creating new assembly with complex payload");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ESKDNumbers.Add(dto.EskdNumber);
                await _context.SaveChangesAsync();

                dto.Assembly.EskdNumberId = dto.EskdNumber.Id;
                _context.Assemblies.Add(dto.Assembly);
                await _context.SaveChangesAsync();

                if (dto.ParentAssemblyIds != null && dto.ParentAssemblyIds.Any())
                {
                    foreach (var parentId in dto.ParentAssemblyIds)
                    {
                        _context.AssemblyParents.Add(new AssemblyParent { ChildAssemblyId = dto.Assembly.Id, ParentAssemblyId = parentId });
                    }
                }

                if (dto.RelatedProductIds != null && dto.RelatedProductIds.Any())
                {
                    foreach (var productId in dto.RelatedProductIds)
                    {
                        _context.ProductAssemblies.Add(new ProductAssembly { AssemblyId = dto.Assembly.Id, ProductId = productId });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction(nameof(Get), new { id = dto.Assembly.Id }, dto.Assembly);
            }
            catch (System.Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating assembly with complex payload");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Получает родительские сборки для данной сборки.
        /// </summary>
        /// <param name="id">Идентификатор сборки.</param>
        /// <returns>Список родительских сборок.</returns>
        [HttpGet("{id}/parents")]
        public async Task<ActionResult<IEnumerable<Assembly>>> GetParentAssemblies(int id)
        {
            _logger.LogInformation($"Getting parent assemblies for assembly with id {id}");
            return await _context.AssemblyParents
                .Where(ap => ap.ChildAssemblyId == id)
                .Include(ap => ap.ParentAssembly.EskdNumber.ClassNumber)
                .Select(ap => ap.ParentAssembly)
                .ToListAsync();
        }

        /// <summary>
        /// Получает продукты, связанные с данной сборкой.
        /// </summary>
        /// <param name="id">Идентификатор сборки.</param>
        /// <returns>Список связанных продуктов.</returns>
        [HttpGet("{id}/products")]
        public async Task<ActionResult<IEnumerable<Product>>> GetRelatedProducts(int id)
        {
            _logger.LogInformation($"Getting related products for assembly with id {id}");
            return await _context.ProductAssemblies
                .Where(pa => pa.AssemblyId == id)
                .Include(pa => pa.Product.EskdNumber.ClassNumber)
                .Select(pa => pa.Product)
                .ToListAsync();
        }

        /// <summary>
        /// Обновляет родительские сборки для данной сборки.
        /// </summary>
        /// <param name="id">Идентификатор сборки.</param>
        /// <param name="parentIds">Список идентификаторов родительских сборок.</param>
        /// <returns>NoContent в случае успеха, NotFound, если сборка не существует.</returns>
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

        /// <summary>
        /// Обновляет продукты, связанные с данной сборкой.
        /// </summary>
        /// <param name="id">Идентификатор сборки.</param>
        /// <param name="productIds">Список идентификаторов продуктов.</param>
        /// <returns>NoContent в случае успеха, NotFound, если сборка не существует.</returns>
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