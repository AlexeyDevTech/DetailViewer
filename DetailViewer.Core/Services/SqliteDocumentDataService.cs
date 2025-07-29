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
        private readonly ApplicationDbContext _dbContext;
        private readonly IClassifierProvider _classifierProvider;

        public SqliteDocumentDataService(ApplicationDbContext dbContext, IClassifierProvider classifierProvider)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _classifierProvider = classifierProvider ?? throw new ArgumentNullException(nameof(classifierProvider));
            _dbContext.Database.EnsureCreated();
        }

        public async Task<List<DocumentDetailRecord>> GetAllRecordsAsync()
        {
            return await _dbContext.DocumentRecords
                .Include(r => r.ESKDNumber)
                .ThenInclude(e => e.ClassNumber)
                .ToListAsync();
        }

        public async Task AddRecordAsync(DocumentDetailRecord record, List<int> assemblyIds)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                _dbContext.DocumentRecords.Add(record);
                await _dbContext.SaveChangesAsync();

                if (assemblyIds?.Any() == true)
                {
                    var assemblyDetails = assemblyIds.Select(id => new AssemblyDetail
                    {
                        AssemblyId = id,
                        DetailId = record.Id
                    }).ToList();
                    _dbContext.AssemblyDetails.AddRange(assemblyDetails);
                    await _dbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateRecordAsync(DocumentDetailRecord record, List<int> assemblyIds)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                _dbContext.DocumentRecords.Update(record);

                await _dbContext.AssemblyDetails
                    .Where(ad => ad.DetailId == record.Id)
                    .ExecuteDeleteAsync();

                if (assemblyIds?.Any() == true)
                {
                    var newLinks = assemblyIds.Select(id => new AssemblyDetail
                    {
                        AssemblyId = id,
                        DetailId = record.Id
                    }).ToList();
                    _dbContext.AssemblyDetails.AddRange(newLinks);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteRecordAsync(int recordId)
        {
            await _dbContext.DocumentRecords
                .Where(r => r.Id == recordId)
                .ExecuteDeleteAsync();
        }

        public async Task<List<Assembly>> GetAssembliesAsync()
        {
            return await _dbContext.Assemblies
                .Include(a => a.EskdNumber)
                .ThenInclude(e => e.ClassNumber)
                .ToListAsync();
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            return await _dbContext.Products
                .Include(p => p.EskdNumber)
                .ThenInclude(e => e.ClassNumber)
                .ToListAsync();
        }

        public async Task DeleteAssemblyAsync(int assemblyId)
        {
            await _dbContext.Assemblies
                .Where(a => a.Id == assemblyId)
                .ExecuteDeleteAsync();
        }

        public async Task DeleteProductAsync(int productId)
        {
            await _dbContext.Products
                .Where(p => p.Id == productId)
                .ExecuteDeleteAsync();
        }

        public async Task AddAssemblyAsync(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            _dbContext.Assemblies.Add(assembly);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAssemblyAsync(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            _dbContext.Assemblies.Update(assembly);
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddProductAsync(Product product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));

            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateProductAsync(Product product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));

            _dbContext.Products.Update(product);
            await _dbContext.SaveChangesAsync();
        }

        public async Task CreateProductWithAssembliesAsync(Product product, List<int> parentAssemblyIds)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // 1. Добавляем продукт в контекст и сохраняем, чтобы получить его Id
                _dbContext.Products.Add(product);
                await _dbContext.SaveChangesAsync();

                // 2. Если есть сборки для связи, создаем записи в таблице ProductAssemblies
                if (parentAssemblyIds?.Any() == true)
                {
                    var newLinks = parentAssemblyIds.Select(assemblyId => new ProductAssembly
                    {
                        ProductId = product.Id, // Используем Id только что созданного продукта
                        AssemblyId = assemblyId
                    }).ToList();
                    _dbContext.ProductAssemblies.AddRange(newLinks);
                    await _dbContext.SaveChangesAsync();
                }

                // 3. Подтверждаем транзакцию
                await transaction.CommitAsync();
            }
            catch
            {
                // В случае ошибки откатываем все изменения
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Classifier> GetOrCreateClassifierAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code) || !int.TryParse(code, out int classifierNumber))
            {
                return null;
            }

            var classifier = await _dbContext.Classifiers
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
                _dbContext.Classifiers.Add(classifier);
                await _dbContext.SaveChangesAsync();
            }

            return classifier;
        }

        public async Task<List<Product>> GetProductsByAssemblyId(int assemblyId)
        {
            return await _dbContext.ProductAssemblies
                .Where(pa => pa.AssemblyId == assemblyId)
                .Join(_dbContext.Products.Include(p => p.EskdNumber).ThenInclude(e => e.ClassNumber),
                    pa => pa.ProductId,
                    p => p.Id,
                    (pa, p) => p)
                .ToListAsync();
        }

        public async Task<List<Assembly>> GetParentAssembliesAsync(int assemblyId)
        {
            return await _dbContext.AssemblyParents
                .Where(ap => ap.ChildAssemblyId == assemblyId)
                .Join(_dbContext.Assemblies.Include(a => a.EskdNumber).ThenInclude(e => e.ClassNumber),
                    ap => ap.ParentAssemblyId,
                    a => a.Id,
                    (ap, a) => a)
                .ToListAsync();
        }

        public async Task UpdateAssemblyParentAssembliesAsync(int assemblyId, List<Assembly> parentAssemblies)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // 1. Удаляем записи напрямую из БД
                await _dbContext.AssemblyParents
                    .Where(ap => ap.ChildAssemblyId == assemblyId)
                    .ExecuteDeleteAsync();

                // 2. Вручную отсоединяем отслеживаемые сущности, чтобы избежать конфликта
                var trackedEntries = _dbContext.ChangeTracker.Entries<AssemblyParent>()
                    .Where(e => e.Entity.ChildAssemblyId == assemblyId)
                    .ToList();
                foreach (var entry in trackedEntries)
                {
                    entry.State = EntityState.Detached;
                }

                // 3. Добавляем новые связи
                if (parentAssemblies?.Any() == true)
                {
                    var newLinks = parentAssemblies.Select(parent => new AssemblyParent
                    {
                        ParentAssemblyId = parent.Id,
                        ChildAssemblyId = assemblyId
                    }).ToList();
                    _dbContext.AssemblyParents.AddRange(newLinks);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateAssemblyRelatedProductsAsync(int assemblyId, List<Product> relatedProducts)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                await _dbContext.ProductAssemblies
                    .Where(pa => pa.AssemblyId == assemblyId)
                    .ExecuteDeleteAsync();

                if (relatedProducts?.Any() == true)
                {
                    var newLinks = relatedProducts.Select(product => new ProductAssembly
                    {
                        ProductId = product.Id,
                        AssemblyId = assemblyId
                    }).ToList();
                    _dbContext.ProductAssemblies.AddRange(newLinks);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateProductParentAssembliesAsync(int productId, List<Assembly> parentAssemblies)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // 1. Удаляем записи напрямую из БД
                await _dbContext.ProductAssemblies
                    .Where(pa => pa.ProductId == productId)
                    .ExecuteDeleteAsync();

                // 2. Вручную отсоединяем отслеживаемые сущности, чтобы избежать конфликта
                var trackedEntries = _dbContext.ChangeTracker.Entries<ProductAssembly>()
                    .Where(e => e.Entity.ProductId == productId)
                    .ToList();
                foreach (var entry in trackedEntries)
                {
                    entry.State = EntityState.Detached;
                }

                // 3. Добавляем новые связи
                if (parentAssemblies?.Any() == true)
                {
                    var newLinks = parentAssemblies.Select(assembly => new ProductAssembly
                    {
                        ProductId = productId,
                        AssemblyId = assembly.Id
                    }).ToList();
                    _dbContext.ProductAssemblies.AddRange(newLinks);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<Assembly>> GetProductParentAssembliesAsync(int productId)
        {
            return await _dbContext.ProductAssemblies
                .Where(pa => pa.ProductId == productId)
                .Join(_dbContext.Assemblies.Include(a => a.EskdNumber).ThenInclude(e => e.ClassNumber),
                    pa => pa.AssemblyId,
                    a => a.Id,
                    (pa, a) => a)
                .ToListAsync();
        }

        public async Task<List<Product>> GetRelatedProductsAsync(int assemblyId)
        {
            return await _dbContext.ProductAssemblies
                .Where(pa => pa.AssemblyId == assemblyId)
                .Join(_dbContext.Products.Include(p => p.EskdNumber).ThenInclude(e => e.ClassNumber),
                    pa => pa.ProductId,
                    p => p.Id,
                    (pa, p) => p)
                .ToListAsync();
        }

        public async Task<List<Assembly>> GetParentAssembliesForDetailAsync(int detailId)
        {
            var assemblyIds = await _dbContext.AssemblyDetails
                .Where(ad => ad.DetailId == detailId)
                .Select(ad => ad.AssemblyId)
                .ToListAsync();

            return await _dbContext.Assemblies
                .Include(a => a.EskdNumber)
                .ThenInclude(e => e.ClassNumber)
                .Where(a => assemblyIds.Contains(a.Id))
                .ToListAsync();
        }

        public async Task<Assembly> ConvertProductToAssemblyAsync(int productId, List<Product> childProducts)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // 1. Находим исходный продукт
                var productToConvert = await _dbContext.Products
                    .Include(p => p.EskdNumber)
                    .FirstOrDefaultAsync(p => p.Id == productId);

                if (productToConvert == null)
                {
                    throw new KeyNotFoundException($"Продукт с Id={productId} не найден.");
                }

                // 2. Находим родительские сборки для продукта
                var parentAssemblies = await _dbContext.ProductAssemblies
                    .Where(pa => pa.ProductId == productId)
                    .Select(pa => pa.AssemblyId)
                    .ToListAsync();

                // 3. Создаем новую сборку на основе данных продукта
                var newAssembly = new Assembly
                {
                    Name = productToConvert.Name,
                    EskdNumber = productToConvert.EskdNumber,
                    // Копируем другие релевантные поля, если они есть
                };
                _dbContext.Assemblies.Add(newAssembly);
                await _dbContext.SaveChangesAsync(); // Сохраняем, чтобы получить Id новой сборки

                // 4. Перенаправляем родительские связи на новую сборку
                if (parentAssemblies.Any())
                {
                    var newParentLinks = parentAssemblies.Select(parentId => new AssemblyParent
                    {
                        ParentAssemblyId = parentId,
                        ChildAssemblyId = newAssembly.Id
                    }).ToList();
                    _dbContext.AssemblyParents.AddRange(newParentLinks);
                }

                // 5. Добавляем новые дочерние продукты к сборке
                if (childProducts?.Any() == true)
                {
                    var newChildLinks = childProducts.Select(child => new ProductAssembly
                    {
                        AssemblyId = newAssembly.Id,
                        ProductId = child.Id
                    }).ToList();
                    _dbContext.ProductAssemblies.AddRange(newChildLinks);
                }

                // 6. Удаляем старый продукт и его связи
                await _dbContext.ProductAssemblies.Where(pa => pa.ProductId == productId).ExecuteDeleteAsync();
                _dbContext.Products.Remove(productToConvert);
                
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return newAssembly;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}