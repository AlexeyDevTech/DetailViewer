
using DetailViewer.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DetailViewer.Core.Interfaces
{
    public interface IApiClient
    {
        Task<List<T>> GetAsync<T>(string endpoint);
        Task<T> GetByIdAsync<T>(string endpoint, int id);
        Task<T> PostAsync<T>(string endpoint, T data);
        Task PutAsync<T>(string endpoint, int id, T data);
        Task PutAsync<T>(string endpoint, T data);
        Task DeleteAsync(string endpoint, int id);
        

        Task<List<Assembly>> GetParentAssembliesAsync(string entity, int id);
        Task<List<Product>> GetRelatedProductsAsync(int assemblyId);
        Task UpdateParentAssembliesAsync(string entity, int id, List<int> parentIds);
        Task UpdateRelatedProductsAsync(int assemblyId, List<int> productIds);
        Task<Assembly> ConvertProductToAssemblyAsync(int productId, List<int> childProductIds);
    }
}
