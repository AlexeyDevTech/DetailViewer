using DetailViewer.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DetailViewer.Core.Interfaces
{
    public interface IAssemblyService
    {
        Task<List<Assembly>> GetAssembliesAsync();
        Task DeleteAssemblyAsync(int assemblyId);
        Task AddAssemblyAsync(Assembly assembly, List<int> parentAssemblyIds, List<int> relatedProductIds);
        Task UpdateAssemblyAsync(Assembly assembly);
        Task<List<Assembly>> GetParentAssembliesAsync(int assemblyId);
        Task UpdateAssemblyParentAssembliesAsync(int assemblyId, List<Assembly> parentAssemblies);
        Task UpdateAssemblyRelatedProductsAsync(int assemblyId, List<Product> relatedProducts);
        Task<List<Product>> GetRelatedProductsAsync(int assemblyId);
        Task<Assembly> ConvertProductToAssemblyAsync(int productId, List<Product> childProducts);
    }
}