using DetailViewer.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DetailViewer.Core.Interfaces
{
    /// <summary>
    /// Определяет контракт для сервиса, управляющего операциями со сборками.
    /// </summary>
    public interface IAssemblyService
    {
        /// <summary>
        /// Асинхронно получает все сборки.
        /// </summary>
        /// <returns>Список всех сборок.</returns>
        Task<List<Assembly>> GetAssembliesAsync();

        /// <summary>
        /// Асинхронно удаляет сборку по ее ID.
        /// </summary>
        /// <param name="assemblyId">Идентификатор сборки для удаления.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        Task DeleteAssemblyAsync(int assemblyId);

        /// <summary>
        /// Асинхронно добавляет новую сборку со связями.
        /// </summary>
        /// <param name="assembly">Новая сборка.</param>
        /// <param name="parentAssemblyIds">Список ID родительских сборок.</param>
        /// <param name="relatedProductIds">Список ID связанных продуктов.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        Task AddAssemblyAsync(Assembly assembly, List<int> parentAssemblyIds, List<int> relatedProductIds);

        /// <summary>
        /// Асинхронно обновляет существующую сборку.
        /// </summary>
        /// <param name="assembly">Сборка с обновленными данными.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        Task UpdateAssemblyAsync(Assembly assembly);

        /// <summary>
        /// Асинхронно получает список родительских сборок для указанной сборки.
        /// </summary>
        /// <param name="assemblyId">Идентификатор дочерней сборки.</param>
        /// <returns>Список родительских сборок.</returns>
        Task<List<Assembly>> GetParentAssembliesAsync(int assemblyId);

        /// <summary>
        /// Асинхронно обновляет список родительских сборок для указанной сборки.
        /// </summary>
        /// <param name="assemblyId">Идентификатор дочерней сборки.</param>
        /// <param name="parentAssemblies">Новый список родительских сборок.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        Task UpdateAssemblyParentAssembliesAsync(int assemblyId, List<Assembly> parentAssemblies);

        /// <summary>
        /// Асинхронно обновляет список связанных продуктов для сборки.
        /// </summary>
        /// <param name="assemblyId">Идентификатор сборки.</param>
        /// <param name="relatedProducts">Новый список связанных продуктов.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        Task UpdateAssemblyRelatedProductsAsync(int assemblyId, List<Product> relatedProducts);

        /// <summary>
        /// Асинхронно получает список связанных продуктов для сборки.
        /// </summary>
        /// <param name="assemblyId">Идентификатор сборки.</param>
        /// <returns>Список связанных продуктов.</returns>
        Task<List<Product>> GetRelatedProductsAsync(int assemblyId);

        /// <summary>
        /// Асинхронно конвертирует продукт в сборку.
        /// </summary>
        /// <param name="productId">Идентификатор продукта.</param>
        /// <param name="childProducts">Список дочерних продуктов, которые станут деталями.</param>
        /// <returns>Новая созданная сборка.</returns>
        Task<Assembly> ConvertProductToAssemblyAsync(int productId, List<Product> childProducts);
    }
}
