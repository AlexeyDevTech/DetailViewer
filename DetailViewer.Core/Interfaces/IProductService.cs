using DetailViewer.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DetailViewer.Core.Interfaces
{
    public interface IProductService
    {
        Task<List<Product>> GetProductsAsync();
        Task DeleteProductAsync(int productId);
        Task AddProductAsync(Product product, List<int> parentAssemblyIds);
        Task UpdateProductAsync(Product product);
        Task<List<Product>> GetProductsByAssemblyId(int assemblyId);
        Task UpdateProductParentAssembliesAsync(int productId, List<Assembly> parentAssemblies);
        Task<List<Assembly>> GetProductParentAssembliesAsync(int productId);
    }
}