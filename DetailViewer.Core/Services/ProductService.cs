using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DetailViewer.Core.Services
{
    public class ProductService : IProductService
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger _logger;

        public ProductService(IApiClient apiClient, ILogger logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            _logger.Log("Getting all products from API");
            return await _apiClient.GetAsync<Product>(ApiEndpoints.Products);
        }

        public async Task DeleteProductAsync(int productId)
        {
            _logger.Log($"Deleting product via API: {productId}");
            await _apiClient.DeleteAsync(ApiEndpoints.Products, productId);
        }

        public async Task AddProductAsync(Product product, List<int> parentAssemblyIds)
        {
            _logger.Log($"Adding product via API: {product.Name}");
            var payload = new 
            {
                Product = product,
                EskdNumber = product.EskdNumber,
                ParentAssemblyIds = parentAssemblyIds
            };
            await _apiClient.PostAsync(ApiEndpoints.Products, payload);
        }

        public async Task UpdateProductAsync(Product product)
        {
            _logger.Log($"Updating product via API: {product.Name}");
            await _apiClient.PutAsync(ApiEndpoints.Products, product.Id, product);
        }

        public async Task<List<Product>> GetProductsByAssemblyId(int assemblyId)
        {
            _logger.Log($"Getting products by assembly id from API: {assemblyId}");
            return await _apiClient.GetAsync<Product>($"{ApiEndpoints.Assemblies}/{assemblyId}/products");
        }

        public async Task UpdateProductParentAssembliesAsync(int productId, List<Assembly> parentAssemblies)
        {
            _logger.Log($"Updating parent assemblies for product via API: {productId}");
            await _apiClient.PutAsync($"{ApiEndpoints.Products}/{productId}/parents", parentAssemblies.Select(a => a.Id).ToList());
        }

        public async Task<List<Assembly>> GetProductParentAssembliesAsync(int productId)
        {
            _logger.Log($"Getting parent assemblies for product from API: {productId}");
            return await _apiClient.GetAsync<Assembly>($"{ApiEndpoints.Products}/{productId}/parents");
        }
    }
}