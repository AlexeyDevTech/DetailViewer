using DetailViewer.Core.Data;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DetailViewer.Core.Services
{
    public class SqliteDocumentDataService : IDocumentDataService
    {
        private readonly ApplicationDbContext _dbContext;

        public SqliteDocumentDataService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbContext.Database.EnsureCreated();
        }

        public async Task<List<DocumentDetailRecord>> GetAllRecordsAsync()
        {
            return await _dbContext.DocumentRecords.Include(r => r.ESKDNumber).ThenInclude(e => e.ClassNumber).ToListAsync();
        }

        public async Task AddRecordAsync(DocumentDetailRecord record)
        {
            _dbContext.Classifiers.Add(record.ESKDNumber.ClassNumber);
            _dbContext.ESKDNumbers.Add(record.ESKDNumber);
            _dbContext.DocumentRecords.Add(record);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateRecordAsync(DocumentDetailRecord record)
        {
            _dbContext.DocumentRecords.Update(record);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteRecordAsync(int recordId)
        {
            var record = await _dbContext.DocumentRecords.FindAsync(recordId);
            if (record != null)
            {
                _dbContext.DocumentRecords.Remove(record);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}