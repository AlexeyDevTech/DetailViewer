using DetailViewer.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DetailViewer.Core.Interfaces
{
    /// <summary>
    /// Определяет контракт для клиента, взаимодействующего с удаленным API.
    /// </summary>
    public interface IApiClient
    {
        /// <summary>
        /// Асинхронно получает список объектов из указанной конечной точки API.
        /// </summary>
        /// <typeparam name="T">Тип объектов для получения.</typeparam>
        /// <param name="endpoint">Конечная точка API.</param>
        /// <returns>Задача, представляющая асинхронную операцию, с результатом в виде списка объектов.</returns>
        Task<List<T>> GetAsync<T>(string endpoint);

        /// <summary>
        /// Асинхронно получает один объект по его идентификатору из указанной конечной точки API.
        /// </summary>
        /// <typeparam name="T">Тип объекта для получения.</typeparam>
        /// <param name="endpoint">Конечная точка API.</param>
        /// <param name="id">Идентификатор объекта.</param>
        /// <returns>Задача, представляющая асинхронную операцию, с результатом в виде одного объекта.</returns>
        Task<T> GetByIdAsync<T>(string endpoint, int id);

        /// <summary>
        /// Асинхронно отправляет данные методом POST в указанную конечную точку и получает ответ.
        /// </summary>
        /// <typeparam name="TRequest">Тип отправляемых данных.</typeparam>
        /// <typeparam name="TResponse">Тип ожидаемого ответа.</typeparam>
        /// <param name="endpoint">Конечная точка API.</param>
        /// <param name="data">Данные для отправки.</param>
        /// <returns>Задача, представляющая асинхронную операцию, с результатом в виде объекта ответа.</returns>
        Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest data);

        /// <summary>
        /// Асинхронно отправляет данные методом POST в указанную конечную точку без ожидания ответа.
        /// </summary>
        /// <typeparam name="TRequest">Тип отправляемых данных.</typeparam>
        /// <param name="endpoint">Конечная точка API.</param>
        /// <param name="data">Данные для отправки.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        Task PostAsync<TRequest>(string endpoint, TRequest data);

        /// <summary>
        /// Асинхронно обновляет существующий объект по его ID методом PUT.
        /// </summary>
        /// <typeparam name="T">Тип обновляемого объекта.</typeparam>
        /// <param name="endpoint">Конечная точка API.</param>
        /// <param name="id">Идентификатор объекта для обновления.</param>
        /// <param name="data">Новые данные для объекта.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        Task PutAsync<T>(string endpoint, int id, T data);

        /// <summary>
        /// Асинхронно обновляет существующий объект методом PUT (без указания ID в URL).
        /// </summary>
        /// <typeparam name="T">Тип обновляемого объекта.</typeparam>
        /// <param name="endpoint">Конечная точка API.</param>
        /// <param name="data">Новые данные для объекта.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        Task PutAsync<T>(string endpoint, T data);

        /// <summary>
        /// Асинхронно удаляет объект по его ID.
        /// </summary>
        /// <param name="endpoint">Конечная точка API.</param>
        /// <param name="id">Идентификатор объекта для удаления.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        Task DeleteAsync(string endpoint, int id);

        /// <summary>
        /// Асинхронно получает список родительских сборок для указанной сущности.
        /// </summary>
        /// <param name="entity">Тип сущности (например, "product" или "assembly").</param>
        /// <param name="id">Идентификатор сущности.</param>
        /// <returns>Список родительских сборок.</returns>
        Task<List<Assembly>> GetParentAssembliesAsync(string entity, int id);

        /// <summary>
        /// Асинхронно получает список связанных продуктов для указанной сборки.
        /// </summary>
        /// <param name="assemblyId">Идентификатор сборки.</param>
        /// <returns>Список связанных продуктов.</returns>
        Task<List<Product>> GetRelatedProductsAsync(int assemblyId);

        /// <summary>
        /// Асинхронно обновляет список родительских сборок для сущности.
        /// </summary>
        /// <param name="entity">Тип сущности.</param>
        /// <param name="id">Идентификатор сущности.</param>
        /// <param name="parentIds">Список ID родительских сборок.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        Task UpdateParentAssembliesAsync(string entity, int id, List<int> parentIds);

        /// <summary>
        /// Асинхронно обновляет список связанных продуктов для сборки.
        /// </summary>
        /// <param name="assemblyId">Идентификатор сборки.</param>
        /// <param name="productIds">Список ID связанных продуктов.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        Task UpdateRelatedProductsAsync(int assemblyId, List<int> productIds);

        /// <summary>
        /// Асинхронно конвертирует продукт в сборку.
        /// </summary>
        /// <param name="productId">Идентификатор продукта для конвертации.</param>
        /// <param name="childProductIds">Список ID дочерних продуктов, которые станут деталями новой сборки.</param>
        /// <returns>Новая созданная сборка.</returns>
        Task<Assembly> ConvertProductToAssemblyAsync(int productId, List<int> childProductIds);
    }
}
