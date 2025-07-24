using DetailViewer.Core.Data;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DetailViewer.Core.Services
{
    public class SqliteDocumentDataService : IDocumentDataService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IClassifierProvider _classifierProvider;

        public SqliteDocumentDataService(ApplicationDbContext dbContext, IClassifierProvider classifierProvider)
        {
            _dbContext = dbContext;
            _classifierProvider = classifierProvider;
            _dbContext.Database.EnsureCreated();
        }

        public async Task<List<DocumentDetailRecord>> GetAllRecordsAsync()
        {
            return await _dbContext.DocumentRecords.Include(r => r.ESKDNumber).ThenInclude(e => e.ClassNumber).ToListAsync();
        }

        public async Task AddRecordAsync(DocumentDetailRecord record, List<int> assemblyIds)
        {
            _dbContext.DocumentRecords.Add(record);
            await _dbContext.SaveChangesAsync();

            if (assemblyIds != null)
            {
                foreach (var assemblyId in assemblyIds)
                {
                    var assemblyDetail = new AssemblyDetail
                    {
                        AssemblyId = assemblyId,
                        DetailId = record.Id
                    };
                    _dbContext.AssemblyDetails.Add(assemblyDetail);
                }
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateRecordAsync(DocumentDetailRecord record, List<int> assemblyIds)
        {
            _dbContext.DocumentRecords.Update(record);

            var existingLinks = await _dbContext.AssemblyDetails.Where(ad => ad.DetailId == record.Id).ToListAsync();
            _dbContext.AssemblyDetails.RemoveRange(existingLinks);

            if (assemblyIds != null)
            {
                foreach (var assemblyId in assemblyIds)
                {
                    var newLink = new AssemblyDetail
                    {
                        AssemblyId = assemblyId,
                        DetailId = record.Id
                    };
                    _dbContext.AssemblyDetails.Add(newLink);
                }
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
            return await _dbContext.Assemblies.Include(a => a.EskdNumber).ThenInclude(e => e.ClassNumber).ToListAsync();
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            return await _dbContext.Products.Include(p => p.EskdNumber).ThenInclude(e => e.ClassNumber).ToListAsync();
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

        public async Task<Classifier> GetOrCreateClassifierAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code) || !int.TryParse(code, out int classifierNumber))
            {
                return null;
            }

            var classifier = await _dbContext.Classifiers.FirstOrDefaultAsync(c => c.Number == classifierNumber);

            if (classifier == null)
            {
                var classifierInfo = _classifierProvider.GetClassifierByCode(code);
                if (classifierInfo != null)
                {
                    classifier = new Classifier
                    {
                        Number = classifierNumber,
                        Description = classifierInfo.Description
                    };
                    _dbContext.Classifiers.Add(classifier);
                    await _dbContext.SaveChangesAsync();
                }
            }

            return classifier;
        }

        public async Task<List<Product>> GetProductsByAssemblyId(int assemblyId)
        {
            var productIds = await _dbContext.ProductAssemblies
                .Where(pa => pa.AssemblyId == assemblyId)
                .Select(pa => pa.ProductId)
                .ToListAsync();

            return await _dbContext.Products
                .Where(p => productIds.Contains(p.Id))
                .Include(p => p.EskdNumber)
                .ThenInclude(e => e.ClassNumber)
                .ToListAsync();
        }

        public async Task<List<Assembly>> GetParentAssembliesAsync(int assemblyId)
        {
            var parentAssemblyIds = await _dbContext.AssemblyParents
                .Where(ap => ap.ChildAssemblyId == assemblyId)
                .Select(ap => ap.ParentAssemblyId)
                .ToListAsync();

            return await _dbContext.Assemblies
                .Where(a => parentAssemblyIds.Contains(a.Id))
                .Include(a => a.EskdNumber)
                .ThenInclude(e => e.ClassNumber)
                .ToListAsync();
        }

        public async Task UpdateAssemblyParentAssembliesAsync(int assemblyId, List<Assembly> parentAssemblies)
        {
            var existingLinks = await _dbContext.AssemblyParents.Where(ap => ap.ChildAssemblyId == assemblyId).ToListAsync();
            _dbContext.AssemblyParents.RemoveRange(existingLinks);

            if (parentAssemblies != null)
            {
                foreach (var parent in parentAssemblies)
                {
                    var newLink = new AssemblyParent
                    {
                        ParentAssemblyId = parent.Id,
                        ChildAssemblyId = assemblyId
                    };
                    _dbContext.AssemblyParents.Add(newLink);
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAssemblyRelatedProductsAsync(int assemblyId, List<Product> relatedProducts)
        {
            var existingLinks = await _dbContext.ProductAssemblies.Where(pa => pa.AssemblyId == assemblyId).ToListAsync();
            _dbContext.ProductAssemblies.RemoveRange(existingLinks);

            if (relatedProducts != null)
            {
                foreach (var product in relatedProducts)
                {
                    var newLink = new ProductAssembly
                    {
                        ProductId = product.Id,
                        AssemblyId = assemblyId
                    };
                    _dbContext.ProductAssemblies.Add(newLink);
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateProductParentAssembliesAsync(int productId, List<Assembly> parentAssemblies)
        {
            var existingLinks = await _dbContext.ProductAssemblies.Where(pa => pa.ProductId == productId).ToListAsync();
            _dbContext.ProductAssemblies.RemoveRange(existingLinks);

            if (parentAssemblies != null)
            {
                foreach (var assembly in parentAssemblies)
                {
                    var newLink = new ProductAssembly
                    {
                        ProductId = productId,
                        AssemblyId = assembly.Id
                    };
                    _dbContext.ProductAssemblies.Add(newLink);
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Assembly>> GetProductParentAssembliesAsync(int productId)
        {
            var assemblyIds = await _dbContext.ProductAssemblies
                .Where(pa => pa.ProductId == productId)
                .Select(pa => pa.AssemblyId)
                .ToListAsync();

            return await _dbContext.Assemblies
                .Where(a => assemblyIds.Contains(a.Id))
                .Include(a => a.EskdNumber)
                .ThenInclude(e => e.ClassNumber)
                .ToListAsync();
        }

        public async Task<List<Product>> GetRelatedProductsAsync(int assemblyId)
        {
            var productIds = await _dbContext.ProductAssemblies
                .Where(pa => pa.AssemblyId == assemblyId)
                .Select(pa => pa.ProductId)
                .ToListAsync();

            return await _dbContext.Products
                .Where(p => productIds.Contains(p.Id))
                .Include(p => p.EskdNumber)
                .ThenInclude(e => e.ClassNumber)
                .ToListAsync();
        }
    }
}