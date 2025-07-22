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
        Task<List<Assembly>> GetParentAssemblies(int detailId);
        Task<List<DocumentDetailRecord>> GetParentProducts(int detailId);
        Task<List<Product>> GetProductsByAssemblyId(int assemblyId);
    }
}
