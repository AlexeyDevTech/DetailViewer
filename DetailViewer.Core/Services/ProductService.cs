using DetailViewer.Core.Data;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DetailViewer.Core.Services
{
    public class ProductService : IProductService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger _logger;

        public ProductService(IDbContextFactory<ApplicationDbContext> contextFactory, ILogger logger)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            _logger.Log("Getting all products");
            try
            {
                using var dbContext = await _contextFactory.CreateDbContextAsync();
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

        public async Task DeleteProductAsync(int productId)
        {
            _logger.Log($"Deleting product: {productId}");
            try
            {
                using var dbContext = await _contextFactory.CreateDbContextAsync();

                var product = await dbContext.Products.FindAsync(productId);
                if (product == null) return;

                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.Preserve
                };

                var changeLog = new ChangeLog
                {
                    EntityName = nameof(Product),
                    EntityId = productId.ToString(),
                    OperationType = OperationType.Delete,
                    Payload = JsonSerializer.Serialize(product, options), // Serialize before deleting
                    Timestamp = DateTime.UtcNow
                };
                dbContext.ChangeLogs.Add(changeLog);

                dbContext.Products.Remove(product);

                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting product with id {productId}", ex);
                throw;
            }
        }

        public async Task AddProductAsync(Product product)
        {
            _logger.Log($"Adding product: {product.Name}");
            if (product == null) throw new ArgumentNullException(nameof(product));

            try
            {
                using var dbContext = await _contextFactory.CreateDbContextAsync();
                dbContext.Products.Add(product);

                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.Preserve
                };

                var changeLog = new ChangeLog
                {
                    EntityName = nameof(Product),
                    EntityId = product.Id.ToString(),
                    OperationType = OperationType.Create,
                    Payload = JsonSerializer.Serialize(product, options),
                    Timestamp = DateTime.UtcNow
                };
                dbContext.ChangeLogs.Add(changeLog);

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
            _logger.Log($"Updating product: {product.Name}");
            if (product == null) throw new ArgumentNullException(nameof(product));

            try
            {
                using var dbContext = await _contextFactory.CreateDbContextAsync();
                dbContext.Products.Update(product);

                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.Preserve
                };

                var changeLog = new ChangeLog
                {
                    EntityName = nameof(Product),
                    EntityId = product.Id.ToString(),
                    OperationType = OperationType.Update,
                    Payload = JsonSerializer.Serialize(product, options),
                    Timestamp = DateTime.UtcNow
                };
                dbContext.ChangeLogs.Add(changeLog);

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
            _logger.Log($"Creating product with assemblies: {product.Name}");
            if (product == null) throw new ArgumentNullException(nameof(product));

            try
            {
                using var dbContext = await _contextFactory.CreateDbContextAsync();
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

        public async Task<List<Product>> GetProductsByAssemblyId(int assemblyId)
        {
            _logger.Log($"Getting products by assembly id: {assemblyId}");
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
                _logger.LogError($"Error getting products by assembly id {assemblyId}", ex);
                throw;
            }
        }

        public async Task UpdateProductParentAssembliesAsync(int productId, List<Assembly> parentAssemblies)
        {
            _logger.Log($"Updating parent assemblies for product: {productId}");
            try
            {
                using var dbContext = await _contextFactory.CreateDbContextAsync();
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

                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.Preserve
                };

                var changeLog = new ChangeLog
                {
                    EntityName = nameof(Product),
                    EntityId = productId.ToString(),
                    OperationType = OperationType.Update,
                    Payload = JsonSerializer.Serialize(await dbContext.Products.FindAsync(productId), options),
                    Timestamp = DateTime.UtcNow
                };
                dbContext.ChangeLogs.Add(changeLog);

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
            _logger.Log($"Getting parent assemblies for product: {productId}");
            try
            {
                using var dbContext = await _contextFactory.CreateDbContextAsync();
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
    }
}
