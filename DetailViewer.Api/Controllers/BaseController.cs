#pragma warning disable CS8602, CS8605

using DetailViewer.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DetailViewer.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController<TEntity> : ControllerBase where TEntity : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly ILogger<BaseController<TEntity>> _logger;

        public BaseController(ApplicationDbContext context, ILogger<BaseController<TEntity>> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/[controller]
        [HttpGet]
        public virtual async Task<ActionResult<IEnumerable<TEntity>>> Get()
        {
            _logger.LogInformation($"Getting all entities of type {typeof(TEntity).Name}");
            return await _context.Set<TEntity>().ToListAsync();
        }

        // GET: api/[controller]/5
        [HttpGet("{id}")]
        public virtual async Task<ActionResult<TEntity>> Get(int id)
        {
            _logger.LogInformation($"Getting entity of type {typeof(TEntity).Name} with id {id}");
            var entity = await _context.Set<TEntity>().FindAsync(id);

            if (entity == null)
            {
                _logger.LogWarning($"Entity of type {typeof(TEntity).Name} with id {id} not found");
                return NotFound();
            }

            return entity;
        }

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

        // POST: api/[controller]
        [HttpPost]
        public virtual async Task<ActionResult<TEntity>> Post(TEntity entity)
        {
            _logger.LogInformation($"Creating new entity of type {typeof(TEntity).Name}");
            _context.Set<TEntity>().Add(entity);
            await _context.SaveChangesAsync();

            var id = entity.GetType().GetProperty("Id")?.GetValue(entity, null);
            _logger.LogInformation($"Entity of type {typeof(TEntity).Name} created with id {id}");
            return CreatedAtAction("Get", new { id }, entity);
        }

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

        protected async Task<bool> EntityExists(int id)
        {
            return await _context.Set<TEntity>().AnyAsync(e => (int)e.GetType().GetProperty("Id").GetValue(e, null) == id);
        }
    }
}
