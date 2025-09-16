using DetailViewer.Api.Data;
using DetailViewer.Api.DTOs;
using DetailViewer.Api.Models;
using Microsoft.AspNetCore.Identity;
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

        //API: api/DocumentDetailRecords/{CompanyCode}
        [HttpGet("{CompanyCode}")]
        public async Task<ActionResult<IEnumerable<DocumentDetailRecord>>> GetFromCompanyCode(string CompanyCode)
        {

            var records = await _context.DocumentRecords
                .Include(d => d.EskdNumber)
                .ThenInclude(s => s.ClassNumber)
                .Where(x => x.EskdNumber.CompanyCode == CompanyCode).ToListAsync();
            if (records == null) return NotFound();
            else return Ok(records);
        }

        /// <summary>
        /// Получает родительские сборки для данной записи о детали документа.
        /// </summary>
        /// <param name="id">Идентификатор записи о детали документа.</param>
        /// <returns>Список родительских сборок.</returns>
        [HttpGet("{id}/parents/assemblies")]
        public async Task<ActionResult<IEnumerable<Assembly>>> GetParentAssemblies(int id)
        {
            _logger.LogInformation($"Getting parent assemblies for document detail record with id {id}");
            
            var record = await _context.DocumentRecords
                .Include(d => d.Assemblies)
                    .ThenInclude(a => a.EskdNumber)
                    .ThenInclude(e => e.ClassNumber)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (record == null)
            {
                return NotFound();
            }

            return Ok(record.Assemblies);
        }
        [HttpGet("{id}/parents/products")]
        public async Task<ActionResult<IEnumerable<Product>>> GetParentProducts(int id)
        {
            _logger.LogInformation($"Getting parent assemblies for document detail record with id {id}");
            
            var record = await _context.DocumentRecords
                .Include(d => d.Products)
                    .ThenInclude(a => a.EskdNumber)
                    .ThenInclude(e => e.ClassNumber)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (record == null)
            {
                return NotFound();
            }

            return Ok(record.Products);
        }

        /// <summary>
        /// Создает новую запись о детали документа с связанным номером ЕСКД и ссылками на сборки.
        /// </summary>
        /// <param name="dto">DTO, содержащий данные для создания записи о детали документа.</param>
        /// <returns>Созданная запись о детали документа.</returns>
        [HttpPost]
        public async Task<ActionResult<DocumentDetailRecord>> Post(DocumentDetailRecordCreateDto dto)
        {
            _logger.LogInformation("Creating new document detail record with complex payload");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (dto.EskdNumber.ClassNumber != null)
                {
                    var existingClassifier = await _context.Classifiers
                        .FirstOrDefaultAsync(c => c.Number == dto.EskdNumber.ClassNumber.Number);

                    if (existingClassifier != null)
                    {
                        dto.EskdNumber.ClassNumber = existingClassifier;
                    }
                    else
                    {
                        _logger.LogInformation($"Classifier with number {dto.EskdNumber.ClassNumber.Number} not found. A new one will be created.");
                    }
                }

                dto.Record.EskdNumber = dto.EskdNumber;
                _context.DocumentRecords.Add(dto.Record);
                await _context.SaveChangesAsync();

                if (dto.AssemblyIds != null && dto.AssemblyIds.Any())
                {
                    var assembliesToAdd = await _context.Assemblies
                        .Where(a => dto.AssemblyIds.Contains(a.Id))
                        .ToListAsync();
                    dto.Record.Assemblies = assembliesToAdd;
                    await _context.SaveChangesAsync();
                }

                // НОВЫЙ БЛОК: Обработка ProductIds
                if (dto.ProductIds != null && dto.ProductIds.Any())
                {
                    var productsToAdd = await _context.Products
                        .Where(p => dto.ProductIds.Contains(p.Id))
                        .ToListAsync();
                    dto.Record.Products = productsToAdd;
                    await _context.SaveChangesAsync(); // Сохраняем изменения для ProductIds
                }

                await transaction.CommitAsync();
                return CreatedAtAction("Get", new { id = dto.Record.Id }, dto.Record);
            }
            catch (System.Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating document detail record with complex payload");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Обновляет существующую запись о детали документа и ее связи со сборками.
        /// </summary>
        /// <param name="id">Идентификатор обновляемой записи.</param>
        /// <param name="dto">DTO с обновленными данными.</param>
        /// <returns>Результат операции.</returns>
        [HttpPut("with-assemblies/{id}")]
        public async Task<IActionResult> PutWithAssemblies(int id, DocumentDetailRecordUpdateDto dto)
        {
            if (dto?.Record == null || id != dto.Record.Id)
                return BadRequest("ID mismatch or empty record");

            var existingRecord = await _context.DocumentRecords
                .Include(d => d.Assemblies) // Включаем связанные сборки
                .Include(d => d.Products) //включаем связанные продукты
                .FirstOrDefaultAsync(d => d.Id == id);

            if (existingRecord == null)
                return NotFound();

            // Обновляем основные свойства записи из DTO
            _context.Entry(existingRecord).CurrentValues.SetValues(dto.Record);

            // Очищаем текущие связи
            existingRecord.Assemblies.Clear();
            existingRecord.Products.Clear();

            // Добавляем новые связи, если они есть
            if (dto.AssemblyIds != null && dto.AssemblyIds.Any())
            {
                var assembliesToAdd = await _context.Assemblies
                    .Where(a => dto.AssemblyIds.Contains(a.Id))
                    .ToListAsync();
                
                foreach (var assembly in assembliesToAdd)
                {
                    existingRecord.Assemblies.Add(assembly);
                }
            }
            // НОВЫЙ БЛОК: Обработка ProductIds
            if (dto.ProductIds != null && dto.ProductIds.Any())
            {
                var productsToAdd = await _context.Products
                    .Where(p => dto.ProductIds.Contains(p.Id))
                    .ToListAsync();
                foreach(var product in productsToAdd) 
                {
                    existingRecord.Products.Add(product);
                }
            }

            try
            {
                // EF Core автоматически определит, какие записи в AssemblyDetails удалить, а какие добавить
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating DocumentDetailRecord with id {id}");
                throw; // В режиме разработки лучше выбрасывать исключение для детального анализа
            }
        }
    }
}