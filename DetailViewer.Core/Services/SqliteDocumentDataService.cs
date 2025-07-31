using DetailViewer.Core.Data;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DetailViewer.Core.Services
{
    public class SqliteDocumentDataService : IDocumentDataService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly IClassifierProvider _classifierProvider;
        private readonly ILogger _logger;

        public SqliteDocumentDataService(IDbContextFactory<ApplicationDbContext> contextFactory, IClassifierProvider classifierProvider, ILogger logger)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _classifierProvider = classifierProvider ?? throw new ArgumentNullException(nameof(classifierProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<DocumentDetailRecord>> GetAllRecordsAsync()
        {
            try
            {
                using var dbContext = _contextFactory.CreateDbContext();
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
            if (record == null) throw new ArgumentNullException(nameof(record));
            
            try
            {
                using var dbContext = _contextFactory.CreateDbContext();
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
            if (record == null) throw new ArgumentNullException(nameof(record));

            try
            {
                using var dbContext = _contextFactory.CreateDbContext();
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
            try
            {
                using var dbContext = _contextFactory.CreateDbContext();
                await dbContext.DocumentRecords
                    .Where(r => r.Id == recordId)
                    .ExecuteDeleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting record with id {recordId}", ex);
                throw;
            }
        }

        public async Task<List<Assembly>> GetAssembliesAsync()
        {
            try
            {
                using var dbContext = _contextFactory.CreateDbContext();
                return await dbContext.Assemblies
                    .Include(a => a.EskdNumber)
                    .ThenInclude(e => e.ClassNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting assemblies", ex);
                throw;
            }
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            try
            {
                using var dbContext = _contextFactory.CreateDbContext();
                return await dbContext.Products
                    .Include(p => p.EskdNumber)
                    .ThenInclude(e => e.ClassNumber)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error getting products", ex);
                throw;
            }
        }

        public async Task DeleteAssemblyAsync(int assemblyId)
        {
            try
            {
                using var dbContext = _contextFactory.CreateDbContext();
                await dbContext.Assemblies
                    .Where(a => a.Id == assemblyId)
                    .ExecuteDeleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting assembly with id {assemblyId}", ex);
                throw;
            }
        }

        public async Task DeleteProductAsync(int productId)
        {
            try
            {
                using var dbContext = _contextFactory.CreateDbContext();
                await dbContext.Products
                    .Where(p => p.Id == productId)
                    .ExecuteDeleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting product with id {productId}", ex);
                throw;
            }
        }

        public async Task AddAssemblyAsync(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            try
            {
                using var dbContext = _contextFactory.CreateDbContext();
                dbContext.Assemblies.Add(assembly);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adding assembly", ex);
                throw;
            }
        }

        public async Task UpdateAssemblyAsync(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            try
            {
                using var dbContext = _contextFactory.CreateDbContext();
                dbContext.Assemblies.Update(assembly);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating assembly", ex);
                throw;
            }
        }

        public async Task AddProductAsync(Product product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));

            try
            {
                using var dbContext = _contextFactory.CreateDbContext();
                dbContext.Products.Add(product);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error adding product", ex);
                throw;
            }
        }

        public async Task UpdateProductAsync(Product product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));

            try
            {
                using var dbContext = _contextFactory.CreateDbContext();
                dbContext.Products.Update(product);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating product", ex);
                throw;
            }
        }

        public async Task CreateProductWithAssembliesAsync(Product product, List<int> parentAssemblyIds)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));

            try
            {
                using var dbContext = _contextFactory.CreateDbContext();
                await using var transaction = await dbContext.Database.BeginTransactionAsync();
                
                dbContext.Products.Add(product);
                await dbContext.SaveChangesAsync();

                if (parentAssemblyIds?.Any() == true)
                {
                    var newLinks = parentAssemblyIds.Select(assemblyId => new ProductAssembly
                    {
                        ProductId = product.Id,
                        AssemblyId = assemblyId
                    }).ToList();
                    dbContext.ProductAssemblies.AddRange(newLinks);
                    await dbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating product with assemblies", ex);
                throw;
            }
        }

        public async Task<Classifier> GetOrCreateClassifierAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code) || !int.TryParse(code, out int classifierNumber))
            {
                return null;
            }

            try
            {
                using var dbContext = _contextFactory.CreateDbContext();
                var classifier = await dbContext.Classifiers
                    .FirstOrDefaultAsync(c => c.Number == classifierNumber);

                if (classifier == null)
                {
                    var classifierInfo = _classifierProvider.GetClassifierByCode(code);
                    if (classifierInfo == null) return null;

                    classifier = new Classifier
                    {
                        Number = classifierNumber,
                        Description = classifierInfo.Description
                    };
                    dbContext.Classifiers.Add(classifier);
                    await dbContext.SaveChangesAsync();
                }

                return classifier;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting or creating classifier with code {code}", ex);
                throw;
            }
        }

        public async Task<List<Product>> GetProductsByAssemblyId(int assemblyId)
        {
            try
            {
                using var dbContext = _contextFactory.CreateDbContext();
                return await dbContext.ProductAssemblies
                    .Where(pa => pa.AssemblyId == assemblyId)
                    .Join(dbContext.Products.Include(p => p.EskdNumber).ThenInclude(e => e.ClassNumber),
                        pa => pa.ProductId,
                        p => p.Id,
                        (pa, p) => p)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting products by assembly id {assemblyId}", ex);
                throw;
            }
        }

        public async Task<List<Assembly>> GetParentAssembliesAsync(int assemblyId)
        {
            try
            {
                using var dbContext = _contextFactory.CreateDbContext();
                return await dbContext.AssemblyParents
                    .Where(ap => ap.ChildAssemblyId == assemblyId)
                    .Join(dbContext.Assemblies.Include(a => a.EskdNumber).ThenInclude(e => e.ClassNumber),
                        ap => ap.ParentAssemblyId,
                        a => a.Id,
                        (ap, a) => a)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting parent assemblies for assembly with id {assemblyId}", ex);
                throw;
            }
        }

        public async Task UpdateAssemblyParentAssembliesAsync(int assemblyId, List<Assembly> parentAssemblies)
        {
            try
            {
                using var dbContext = _contextFactory.CreateDbContext();
                await using var transaction = await dbContext.Database.BeginTransactionAsync();
                
                await dbContext.AssemblyParents
                    .Where(ap => ap.ChildAssemblyId == assemblyId)
                    .ExecuteDeleteAsync();

                var trackedEntries = dbContext.ChangeTracker.Entries<AssemblyParent>()
                    .Where(e => e.Entity.ChildAssemblyId == assemblyId)
                    .ToList();
                foreach (var entry in trackedEntries)
                {
                    entry.State = EntityState.Detached;
                }

                if (parentAssemblies?.Any() == true)
                {
                    var newLinks = parentAssemblies.Select(parent => new AssemblyParent
                    {
                        ParentAssemblyId = parent.Id,
                        ChildAssemblyId = assemblyId
                    }).ToList();
                    dbContext.AssemblyParents.AddRange(newLinks);
                }

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating parent assemblies for assembly with id {assemblyId}", ex);
                throw;
            }
        }

        public async Task UpdateAssemblyRelatedProductsAsync(int assemblyId, List<Product> relatedProducts)
        {
            try
            {
                using var dbContext = _contextFactory.CreateDbContext();
                await using var transaction = await dbContext.Database.BeginTransactionAsync();
                
                await dbContext.ProductAssemblies
                    .Where(pa => pa.AssemblyId == assemblyId)
                    .ExecuteDeleteAsync();

                if (relatedProducts?.Any() == true)
                {
                    var newLinks = relatedProducts.Select(product => new ProductAssembly
                    {
                        ProductId = product.Id,
                        AssemblyId = assemblyId
                    }).ToList();
                    dbContext.ProductAssemblies.AddRange(newLinks);
                }

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating related products for assembly with id {assemblyId}", ex);
                throw;
            }
        }

        public async Task UpdateProductParentAssembliesAsync(int productId, List<Assembly> parentAssemblies)
        {
            try
            {
                using var dbContext = _contextFactory.CreateDbContext();
                await using var transaction = await dbContext.Database.BeginTransactionAsync();
                
                var existingLinks = await dbContext.ProductAssemblies
                    .Where(pa => pa.ProductId == productId)
                    .ToListAsync();

                dbContext.ProductAssemblies.RemoveRange(existingLinks);

                if (parentAssemblies?.Any() == true)
                {
                    var newLinks = parentAssemblies.Select(assembly => new ProductAssembly
                    {
                        ProductId = productId,
                        AssemblyId = assembly.Id
                    }).ToList();
                    dbContext.ProductAssemblies.AddRange(newLinks);
                }

                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating parent assemblies for product with id {productId}", ex);
                throw;
            }
        }

        public async Task<List<Assembly>> GetProductParentAssembliesAsync(int productId)
        {
            try
            {
                using var dbContext = _contextFactory.CreateDbContext();
                return await dbContext.ProductAssemblies
                    .Where(pa => pa.ProductId == productId)
                    .Join(dbContext.Assemblies.Include(a => a.EskdNumber).ThenInclude(e => e.ClassNumber),
                        pa => pa.AssemblyId,
                        a => a.Id,
                        (pa, a) => a)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting parent assemblies for product with id {productId}", ex);
                throw;
            }
        }

        public async Task<List<Product>> GetRelatedProductsAsync(int assemblyId)
        {
            try
            {
                using var dbContext = _contextFactory.CreateDbContext();
                return await dbContext.ProductAssemblies
                    .Where(pa => pa.AssemblyId == assemblyId)
                    .Join(dbContext.Products.Include(p => p.EskdNumber).ThenInclude(e => e.ClassNumber),
                        pa => pa.ProductId,
                        p => p.Id,
                        (pa, p) => p)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting related products for assembly with id {assemblyId}", ex);
                throw;
            }
        }

        public async Task<List<Assembly>> GetParentAssembliesForDetailAsync(int detailId)
        {
            try
            {
                using var dbContext = _contextFactory.CreateDbContext();
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

        public async Task<Assembly> ConvertProductToAssemblyAsync(int productId, List<Product> childProducts)
        {
            try
            {
                using var dbContext = _contextFactory.CreateDbContext();
                await using var transaction = await dbContext.Database.BeginTransactionAsync();
                
                var productToConvert = await dbContext.Products
                    .Include(p => p.EskdNumber)
                    .FirstOrDefaultAsync(p => p.Id == productId);

                if (productToConvert == null)
                {
                    throw new KeyNotFoundException($"Product with Id={productId} not found.");
                }

                var parentAssemblies = await dbContext.ProductAssemblies
                    .Where(pa => pa.ProductId == productId)
                    .Select(pa => pa.AssemblyId)
                    .ToListAsync();

                var newAssembly = new Assembly
                {
                    Name = productToConvert.Name,
                    EskdNumber = productToConvert.EskdNumber,
                };
                dbContext.Assemblies.Add(newAssembly);
                await dbContext.SaveChangesAsync();

                if (parentAssemblies.Any())
                {
                    var newParentLinks = parentAssemblies.Select(parentId => new AssemblyParent
                    {
                        ParentAssemblyId = parentId,
                        ChildAssemblyId = newAssembly.Id
                    }).ToList();
                    dbContext.AssemblyParents.AddRange(newParentLinks);
                }

                if (childProducts?.Any() == true)
                {
                    var newChildLinks = childProducts.Select(child => new ProductAssembly
                    {
                        AssemblyId = newAssembly.Id,
                        ProductId = child.Id
                    }).ToList();
                    dbContext.ProductAssemblies.AddRange(newChildLinks);
                }

                await dbContext.ProductAssemblies.Where(pa => pa.ProductId == productId).ExecuteDeleteAsync();
                dbContext.Products.Remove(productToConvert);
                
                await dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return newAssembly;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error converting product to assembly for product with id {productId}", ex);
                throw;
            }
        }
    }
}
