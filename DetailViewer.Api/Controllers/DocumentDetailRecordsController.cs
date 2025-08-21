using DetailViewer.Api.Data;
using DetailViewer.Api.Models;
using DetailViewer.Api.Models.Dtos;
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
        public DocumentDetailRecordsController(ApplicationDbContext context, ILogger<DocumentDetailRecordsController> logger) : base(context, logger)
        {
        }

        public override async Task<ActionResult<IEnumerable<DocumentDetailRecord>>> Get()
        {
            _logger.LogInformation("Getting all document detail records");
            return await _context.DocumentRecords
                .Include(r => r.ESKDNumber)
                .ThenInclude(e => e.ClassNumber)
                .ToListAsync();
        }

        public override async Task<ActionResult<DocumentDetailRecord>> Get(int id)
        {
            _logger.LogInformation($"Getting document detail record with id {id}");
            var entity = await _context.DocumentRecords
                .Include(r => r.ESKDNumber)
                .ThenInclude(e => e.ClassNumber)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (entity == null)
            {
                _logger.LogWarning($"Document detail record with id {id} not found");
                return NotFound();
            }

            return entity;
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

        [HttpPost]
        public async Task<ActionResult<DocumentDetailRecord>> Post(DocumentDetailRecordDto dto)
        {
            _logger.LogInformation("Creating new document detail record");
            using var transaction = await _context.Database.BeginTransactionAsync();

            var record = new DocumentDetailRecord
            {
                Name = dto.Name,
                ESKDNumber = dto.ESKDNumber,
                //... copy other properties from dto to record
            };

            _context.DocumentRecords.Add(record);
            await _context.SaveChangesAsync();

            if (dto.ParentAssemblyIds != null)
            {
                foreach (var parentId in dto.ParentAssemblyIds)
                {
                    _context.AssemblyDetails.Add(new AssemblyDetail { DetailId = record.Id, AssemblyId = parentId });
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation($"Document detail record created with id {record.Id}");
            return CreatedAtAction(nameof(Get), new { id = record.Id }, record);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, DocumentDetailRecordDto dto)
        {
            _logger.LogInformation($"Updating document detail record with id {id}");
            if (id != dto.Id)
            {
                return BadRequest();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            var record = await _context.DocumentRecords.FindAsync(id);
            if (record == null)
            {
                _logger.LogWarning($"Document detail record with id {id} not found");
                return NotFound();
            }

            record.Name = dto.Name;
            record.ESKDNumber = dto.ESKDNumber;
            //... copy other properties

            var existingParents = await _context.AssemblyDetails.Where(ad => ad.DetailId == id).ToListAsync();
            _context.AssemblyDetails.RemoveRange(existingParents);

            if (dto.ParentAssemblyIds != null)
            {
                foreach (var parentId in dto.ParentAssemblyIds)
                {
                    _context.AssemblyDetails.Add(new AssemblyDetail { DetailId = id, AssemblyId = parentId });
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return NoContent();
        }
    }
}
