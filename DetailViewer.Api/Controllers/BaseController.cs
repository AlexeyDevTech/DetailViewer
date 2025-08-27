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
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController<TEntity> : ControllerBase where TEntity : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly ILogger<BaseController<TEntity>> _logger;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="BaseController{TEntity}"/>.
        /// </summary>
        /// <param name="context">Контекст базы данных приложения.</param>
        /// <param name="logger">Логгер для контроллера.</param>
        public BaseController(ApplicationDbContext context, ILogger<BaseController<TEntity>> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Получает все сущности типа <typeparamref name="TEntity"/>.
        /// </summary>
        /// <returns>Список сущностей.</returns>
        // GET: api/[controller]
        [HttpGet]
        public virtual async Task<ActionResult<IEnumerable<TEntity>>> Get()
        {
            _logger.LogInformation($"Getting all entities of type {typeof(TEntity).Name}");
            IQueryable<TEntity> query = _context.Set<TEntity>();

            if (typeof(TEntity) == typeof(DocumentDetailRecord))
            {
                query = (IQueryable<TEntity>)_context.Set<DocumentDetailRecord>().Include(d => d.EskdNumber).ThenInclude(e => e.ClassNumber);
            }
            else if (typeof(TEntity) == typeof(Assembly))
            {
                query = (IQueryable<TEntity>)_context.Set<Assembly>().Include(a => a.EskdNumber).ThenInclude(e => e.ClassNumber);
            }
            else if (typeof(TEntity) == typeof(Product))
            {
                query = (IQueryable<TEntity>)_context.Set<Product>().Include(p => p.EskdNumber).ThenInclude(e => e.ClassNumber);
            }

            return await query.ToListAsync();
        }

        /// <summary>
        /// Получает конкретную сущность по ее идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор сущности для получения.</param>
        /// <returns>Сущность с указанным идентификатором, или NotFound, если не найдена.</returns>
        // GET: api/[controller]/5
        [HttpGet("{id}")]
        public virtual async Task<ActionResult<TEntity>> Get(int id)
        {
            _logger.LogInformation($"Getting entity of type {typeof(TEntity).Name} with id {id}");
            
            IQueryable<TEntity> query = _context.Set<TEntity>();

            if (typeof(TEntity) == typeof(DocumentDetailRecord))
            {
                query = (IQueryable<TEntity>)_context.Set<DocumentDetailRecord>().Include(d => d.EskdNumber).ThenInclude(e => e.ClassNumber);
            }
            else if (typeof(TEntity) == typeof(Assembly))
            {
                query = (IQueryable<TEntity>)_context.Set<Assembly>().Include(a => a.EskdNumber).ThenInclude(e => e.ClassNumber);
            }
            else if (typeof(TEntity) == typeof(Product))
            {
                query = (IQueryable<TEntity>)_context.Set<Product>().Include(p => p.EskdNumber).ThenInclude(e => e.ClassNumber);
            }

            var entity = await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);

            if (entity == null)
            {
                _logger.LogWarning($"Entity of type {typeof(TEntity).Name} with id {id} not found");
                return NotFound();
            }

            return entity;
        }

        /// <summary>
        /// Обновляет существующую сущность.
        /// </summary>
        /// <param name="id">Идентификатор сущности для обновления.</param>
        /// <param name="entity">Объект сущности с обновленными значениями.</param>
        /// <returns>NoContent в случае успеха, BadRequest при несоответствии ID, NotFound, если сущность не существует.</returns>
        // PUT: api/[controller]/5
        [HttpPut("{id}")]
        public virtual async Task<IActionResult> Put(int id, TEntity entity)
        {
            _logger.LogInformation($"Updating entity of type {typeof(TEntity).Name} with id {id}");
            var entityId = entity.GetType().GetProperty("Id")?.GetValue(entity, null);
            if (entityId == null || id != (int)entityId)
            {
                return BadRequest();
            }

            _context.Entry(entity).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, $"Error updating entity of type {typeof(TEntity).Name} with id {id}");
                if (!await EntityExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Создает новую сущность.
        /// </summary>
        /// <param name="entity">Объект сущности для создания.</param>
        /// <returns>Созданная сущность с ее идентификатором.</returns>
        protected virtual async Task<ActionResult<TEntity>> Post(TEntity entity)
        {
            _logger.LogInformation($"Creating new entity of type {typeof(TEntity).Name}");
            _context.Set<TEntity>().Add(entity);
            await _context.SaveChangesAsync();

            var id = entity.GetType().GetProperty("Id")?.GetValue(entity, null);
            _logger.LogInformation($"Entity of type {typeof(TEntity).Name} created with id {id}");
            return CreatedAtAction("Get", new { id }, entity);
        }

        /// <summary>
        /// Удаляет конкретную сущность по ее идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор сущности для удаления.</param>
        /// <returns>NoContent в случае успеха, NotFound, если сущность не существует.</returns>
        // DELETE: api/[controller]/5
        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation($"Deleting entity of type {typeof(TEntity).Name} with id {id}");
            var entity = await _context.Set<TEntity>().FindAsync(id);
            if (entity == null)
            {
                _logger.LogWarning($"Entity of type {typeof(TEntity).Name} with id {id} not found");
                return NotFound();
            }

            _context.Set<TEntity>().Remove(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Entity of type {typeof(TEntity).Name} with id {id} deleted");
            return NoContent();
        }

        /// <summary>
        /// Проверяет, существует ли сущность с указанным идентификатором.
        /// </summary>
        /// <param name="id">Идентификатор сущности для проверки.</param>
        /// <returns>True, если сущность существует, иначе false.</returns>
        protected async Task<bool> EntityExists(int id)
        {
            return await _context.Set<TEntity>().AnyAsync(e => EF.Property<int>(e, "Id") == id);
        }
    }
}