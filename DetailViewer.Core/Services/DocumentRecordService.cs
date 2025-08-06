using DetailViewer.Core.Data;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DetailViewer.Core.Services
{
    public class DocumentRecordService : IDocumentRecordService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger _logger;

        public DocumentRecordService(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger logger)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<DocumentDetailRecord>> GetAllRecordsAsync()
        {
            _logger.Log("Getting all records");
            try
            {
                using var dbContext = await _contextFactory.CreateDbContextAsync();
                return await dbContext.DocumentRecords
                    .Include(r => r.ESKDNumber)
                    .ThenInclude(e => e.ClassNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting all records", ex);
                throw;
            }
        }

        public async Task AddRecordAsync(DocumentDetailRecord record, List<int> assemblyIds)
        {
            _logger.Log($"Adding record: {record.Name}");
            if (record == null) throw new ArgumentNullException(nameof(record));
            
            try
            {
                using var dbContext = await _contextFactory.CreateDbContextAsync();
                await using var transaction = await dbContext.Database.BeginTransactionAsync();
                
                dbContext.DocumentRecords.Add(record);
                await dbContext.SaveChangesAsync();

                if (assemblyIds?.Any() == true)
                {
                    var assemblyDetails = assemblyIds.Select(id => new AssemblyDetail
                    {
                        AssemblyId = id,
                        DetailId = record.Id
                    }).ToList();
                    dbContext.AssemblyDetails.AddRange(assemblyDetails);
                    await dbContext.SaveChangesAsync();
                }

                var changeLog = new ChangeLog
                {
                    EntityName = nameof(DocumentDetailRecord),
                    EntityId = record.Id.ToString(),
                    OperationType = OperationType.Create,
                    Payload = JsonSerializer.Serialize(record),
                    Timestamp = DateTime.UtcNow
                };
                dbContext.ChangeLogs.Add(changeLog);

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adding record", ex);
                throw;
            }
        }

        public async Task UpdateRecordAsync(DocumentDetailRecord record, List<int> assemblyIds)
        {
            _logger.Log($"Updating record: {record.Name}");
            if (record == null) throw new ArgumentNullException(nameof(record));

            try
            {
                using var dbContext = await _contextFactory.CreateDbContextAsync();
                await using var transaction = await dbContext.Database.BeginTransactionAsync();
                
                dbContext.DocumentRecords.Update(record);

                await dbContext.AssemblyDetails
                    .Where(ad => ad.DetailId == record.Id)
                    .ExecuteDeleteAsync();

                if (assemblyIds?.Any() == true)
                {
                    var newLinks = assemblyIds.Select(id => new AssemblyDetail
                    {
                        AssemblyId = id,
                        DetailId = record.Id
                    }).ToList();
                    dbContext.AssemblyDetails.AddRange(newLinks);
                }

                var changeLog = new ChangeLog
                {
                    EntityName = nameof(DocumentDetailRecord),
                    EntityId = record.Id.ToString(),
                    OperationType = OperationType.Update,
                    Payload = JsonSerializer.Serialize(record),
                    Timestamp = DateTime.UtcNow
                };
                dbContext.ChangeLogs.Add(changeLog);

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating record", ex);
                throw;
            }
        }

        public async Task DeleteRecordAsync(int recordId)
        {
            _logger.Log($"Deleting record: {recordId}");
            try
            {
                using var dbContext = await _contextFactory.CreateDbContextAsync();

                var changeLog = new ChangeLog
                {
                    EntityName = nameof(DocumentDetailRecord),
                    EntityId = recordId.ToString(),
                    OperationType = OperationType.Delete,
                    Timestamp = DateTime.UtcNow
                };
                dbContext.ChangeLogs.Add(changeLog);

                await dbContext.DocumentRecords
                    .Where(r => r.Id == recordId)
                    .ExecuteDeleteAsync();

                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting record with id {recordId}", ex);
                throw;
            }
        }

        public async Task<List<Assembly>> GetParentAssembliesForDetailAsync(int detailId)
        {
            _logger.Log($"Getting parent assemblies for detail: {detailId}");
            try
            {
                using var dbContext = await _contextFactory.CreateDbContextAsync();
                var assemblyIds = await dbContext.AssemblyDetails
                    .Where(ad => ad.DetailId == detailId)
                    .Select(ad => ad.AssemblyId)
                    .ToListAsync();

                return await dbContext.Assemblies
                    .Include(a => a.EskdNumber)
                    .ThenInclude(e => e.ClassNumber)
                    .Where(a => assemblyIds.Contains(a.Id))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting parent assemblies for detail with id {detailId}", ex);
                throw;
            }
        }
    }
}
