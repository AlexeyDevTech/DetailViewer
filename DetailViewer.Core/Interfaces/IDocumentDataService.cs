using DetailViewer.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DetailViewer.Core.Interfaces
{
    public interface IDocumentDataService
    {
        Task<List<DocumentDetailRecord>> GetAllRecordsAsync();
        Task AddRecordAsync(DocumentDetailRecord record, List<int> assemblyIds);
        Task UpdateRecordAsync(DocumentDetailRecord record, List<int> assemblyIds);
        Task DeleteRecordAsync(int recordId);
        Task<List<Assembly>> GetAssembliesAsync();
        Task<List<Product>> GetProductsAsync();
        Task DeleteAssemblyAsync(int assemblyId);
        Task AddAssemblyAsync(Assembly assembly);
        Task UpdateAssemblyAsync(Assembly assembly);
        Task AddProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(int productId);
        Task<Classifier> GetOrCreateClassifierAsync(string code);
        Task<List<Assembly>> GetParentAssembliesAsync(int assemblyId);
        Task<List<Product>> GetProductsByAssemblyId(int assemblyId);
        Task UpdateAssemblyParentAssembliesAsync(int assemblyId, List<Assembly> parentAssemblies);
        Task UpdateAssemblyRelatedProductsAsync(int assemblyId, List<Product> relatedProducts);
        Task UpdateProductParentAssembliesAsync(int productId, List<Assembly> parentAssemblies);
        Task<List<Assembly>> GetProductParentAssembliesAsync(int productId);
        Task<List<Product>> GetRelatedProductsAsync(int assemblyId);
    }
}
