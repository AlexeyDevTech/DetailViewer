using DetailViewer.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DetailViewer.Core.Interfaces
{
    public interface IDocumentDataService
    {
        Task<List<DocumentDetailRecord>> GetAllRecordsAsync();
        Task AddRecordAsync(DocumentDetailRecord record, int? assemblyId);
        Task UpdateRecordAsync(DocumentDetailRecord record, int? assemblyId);
        Task DeleteRecordAsync(int recordId);
        Task<List<Assembly>> GetAssembliesAsync();
        Task<List<Product>> GetProductsAsync();
        Task DeleteAssemblyAsync(int assemblyId);
        Task AddAssemblyAsync(Assembly assembly);
        Task UpdateAssemblyAsync(Assembly assembly);
        Task AddProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(int productId);
    }
}
