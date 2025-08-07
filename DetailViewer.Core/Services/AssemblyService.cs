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
    public class AssemblyService : IAssemblyService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger _logger;

        public AssemblyService(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger logger)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<Assembly>> GetAssembliesAsync()
        {
            _logger.Log("Getting all assemblies");
            try
            {
                using var dbContext = await _contextFactory.CreateDbContextAsync();
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

        public async Task DeleteAssemblyAsync(int assemblyId)
        {
            _logger.Log($"Deleting assembly: {assemblyId}");
            try
            {
                using var dbContext = await _contextFactory.CreateDbContextAsync();

                var assembly = await dbContext.Assemblies.FindAsync(assemblyId);
                if (assembly == null) return;

                var changeLog = new ChangeLog
                {
                    EntityName = nameof(Assembly),
                    EntityId = assemblyId.ToString(),
                    OperationType = OperationType.Delete,
                    Payload = JsonSerializer.Serialize(assembly), // Serialize before deleting
                    Timestamp = DateTime.UtcNow
                };
                dbContext.ChangeLogs.Add(changeLog);

                dbContext.Assemblies.Remove(assembly);

                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting assembly with id {assemblyId}", ex);
                throw;
            }
        }

        public async Task AddAssemblyAsync(Assembly assembly)
        {
            _logger.Log($"Adding assembly: {assembly.Name}");
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            try
            {
                using var dbContext = await _contextFactory.CreateDbContextAsync();
                dbContext.Assemblies.Add(assembly);

                var changeLog = new ChangeLog
                {
                    EntityName = nameof(Assembly),
                    EntityId = assembly.Id.ToString(),
                    OperationType = OperationType.Create,
                    Payload = JsonSerializer.Serialize(assembly),
                    Timestamp = DateTime.UtcNow
                };
                dbContext.ChangeLogs.Add(changeLog);

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
            _logger.Log($"Updating assembly: {assembly.Name}");
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            try
            {
                using var dbContext = await _contextFactory.CreateDbContextAsync();
                dbContext.Assemblies.Update(assembly);

                var changeLog = new ChangeLog
                {
                    EntityName = nameof(Assembly),
                    EntityId = assembly.Id.ToString(),
                    OperationType = OperationType.Update,
                    Payload = JsonSerializer.Serialize(assembly),
                    Timestamp = DateTime.UtcNow
                };
                dbContext.ChangeLogs.Add(changeLog);

                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating assembly", ex);
                throw;
            }
        }

        public async Task<List<Assembly>> GetParentAssembliesAsync(int assemblyId)
        {
            _logger.Log($"Getting parent assemblies for assembly: {assemblyId}");
            try
            {
                using var dbContext = await _contextFactory.CreateDbContextAsync();
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
            _logger.Log($"Updating parent assemblies for assembly: {assemblyId}");
            try
            {
                using var dbContext = await _contextFactory.CreateDbContextAsync();
                await using var transaction = await dbContext.Database.BeginTransactionAsync();
                
                var existingLinks = await dbContext.AssemblyParents
                    .Where(ap => ap.ChildAssemblyId == assemblyId)
                    .ToListAsync();

                dbContext.AssemblyParents.RemoveRange(existingLinks);

                if (parentAssemblies?.Any() == true)
                {
                    var newLinks = parentAssemblies.Select(parent => new AssemblyParent
                    {
                        ParentAssemblyId = parent.Id,
                        ChildAssemblyId = assemblyId
                    }).ToList();
                    dbContext.AssemblyParents.AddRange(newLinks);
                }

                var changeLog = new ChangeLog
                {
                    EntityName = nameof(Assembly),
                    EntityId = assemblyId.ToString(),
                    OperationType = OperationType.Update,
                    Payload = JsonSerializer.Serialize(await dbContext.Assemblies.FindAsync(assemblyId)),
                    Timestamp = DateTime.UtcNow
                };
                dbContext.ChangeLogs.Add(changeLog);

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
            _logger.Log($"Updating related products for assembly: {assemblyId}");
            try
            {
                using var dbContext = await _contextFactory.CreateDbContextAsync();
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

        public async Task<List<Product>> GetRelatedProductsAsync(int assemblyId)
        {
            _logger.Log($"Getting related products for assembly: {assemblyId}");
            try
            {
                using var dbContext = await _contextFactory.CreateDbContextAsync();
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

        public async Task<Assembly> ConvertProductToAssemblyAsync(int productId, List<Product> childProducts)
        {
            _logger.Log($"Converting product to assembly: {productId}");
            try
            {
                using var dbContext = await _contextFactory.CreateDbContextAsync();
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
