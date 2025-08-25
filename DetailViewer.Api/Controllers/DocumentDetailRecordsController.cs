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
                .Include(ad => ad.Assembly.EskdNumber.ClassNumber)
                .Select(ad => ad.Assembly)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<DocumentDetailRecord>> Post(DocumentDetailRecordCreateDto dto)
        {
            _logger.LogInformation("Creating new document detail record with complex payload");

            // 1. Save the ESKDNumber first to get its ID
            _context.ESKDNumbers.Add(dto.EskdNumber);
            await _context.SaveChangesAsync();

            // 2. Assign the new ID to the record and save the record
            dto.Record.EskdNumberId = dto.EskdNumber.Id;
            _context.DocumentRecords.Add(dto.Record);
            await _context.SaveChangesAsync();

            // 3. Link assemblies
            if (dto.AssemblyIds != null && dto.AssemblyIds.Any())
            {
                foreach (var assemblyId in dto.AssemblyIds)
                {
                    var assemblyDetail = new AssemblyDetail
                    {
                        AssemblyId = assemblyId,
                        DetailId = dto.Record.Id
                    };
                    _context.AssemblyDetails.Add(assemblyDetail);
                }
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction("Get", new { id = dto.Record.Id }, dto.Record);
        }
    }
}