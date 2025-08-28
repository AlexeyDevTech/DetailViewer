using DetailViewer.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DetailViewer.Core.Interfaces
{
    /// <summary>
    /// Определяет контракт для сервиса, управляющего операциями с продуктами.
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Асинхронно получает все продукты.
        /// </summary>
        /// <returns>Список всех продуктов.</returns>
        Task<List<Product>> GetProductsAsync();

        /// <summary>
        /// Асинхронно удаляет продукт по его ID.
        /// </summary>
        /// <param name="productId">Идентификатор продукта для удаления.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        Task DeleteProductAsync(int productId);

        /// <summary>
        /// Асинхронно добавляет новый продукт.
        /// </summary>
        /// <param name="product">Новый продукт.</param>
        /// <param name="parentAssemblyIds">Список ID родительских сборок.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        Task AddProductAsync(Product product, List<int> parentAssemblyIds);

        /// <summary>
        /// Асинхронно обновляет существующий продукт.
        /// </summary>
        /// <param name="product">Продукт с обновленными данными.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        Task UpdateProductAsync(Product product);

        /// <summary>
        /// Асинхронно получает список продуктов, входящих в указанную сборку.
        /// </summary>
        /// <param name="assemblyId">Идентификатор сборки.</param>
        /// <returns>Список продуктов.</returns>
        Task<List<Product>> GetProductsByAssemblyId(int assemblyId);

        /// <summary>
        /// Асинхронно обновляет список родительских сборок для продукта.
        /// </summary>
        /// <param name="productId">Идентификатор продукта.</param>
        /// <param name="parentAssemblies">Новый список родительских сборок.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        Task UpdateProductParentAssembliesAsync(int productId, List<Assembly> parentAssemblies);

        /// <summary>
        /// Асинхронно получает список родительских сборок для продукта.
        /// </summary>
        /// <param name="productId">Идентификатор продукта.</param>
        /// <returns>Список родительских сборок.</returns>
        Task<List<Assembly>> GetProductParentAssembliesAsync(int productId);
    }
}
