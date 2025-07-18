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

        public async Task AddRecordAsync(DocumentDetailRecord record, int? assemblyId)
        {
            _dbContext.DocumentRecords.Add(record);
            await _dbContext.SaveChangesAsync();

            if (assemblyId.HasValue)
            {
                var assemblyDetail = new AssemblyDetail
                {
                    AssemblyId = assemblyId.Value,
                    DetailId = record.Id
                };
                _dbContext.AssemblyDetails.Add(assemblyDetail);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateRecordAsync(DocumentDetailRecord record, int? assemblyId)
        {
            _dbContext.DocumentRecords.Update(record);

            var existingLink = await _dbContext.AssemblyDetails.FirstOrDefaultAsync(ad => ad.DetailId == record.Id);
            if (existingLink != null)
            {
                _dbContext.AssemblyDetails.Remove(existingLink);
            }

            if (assemblyId.HasValue)
            {
                var newLink = new AssemblyDetail
                {
                    AssemblyId = assemblyId.Value,
                    DetailId = record.Id
                };
                _dbContext.AssemblyDetails.Add(newLink);
            }

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

        public async Task<List<Assembly>> GetAssembliesAsync()
        {
            return await _dbContext.Assemblies.ToListAsync();
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            return await _dbContext.Products.ToListAsync();
        }

        public async Task DeleteAssemblyAsync(int assemblyId)
        {
            var assembly = await _dbContext.Assemblies.FindAsync(assemblyId);
            if (assembly != null)
            {
                _dbContext.Assemblies.Remove(assembly);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task DeleteProductAsync(int productId)
        {
            var product = await _dbContext.Products.FindAsync(productId);
            if (product != null)
            {
                _dbContext.Products.Remove(product);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task AddAssemblyAsync(Assembly assembly)
        {
            _dbContext.Assemblies.Add(assembly);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAssemblyAsync(Assembly assembly)
        {
            _dbContext.Assemblies.Update(assembly);
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddProductAsync(Product product)
        {
            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateProductAsync(Product product)
        {
            _dbContext.Products.Update(product);
            await _dbContext.SaveChangesAsync();
        }
    }
}