using DetailViewer.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DetailViewer.Core.Interfaces
{
    public interface IDocumentDataService
    {
        /// <summary>
        /// Получает все записи документов с их связанными ESKD-номерами и классификаторами.
        /// </summary>
        /// <returns>Список записей документов.</returns>
        Task<List<DocumentDetailRecord>> GetAllRecordsAsync();

        /// <summary>
        /// Добавляет новую запись документа и связывает её с указанными сборками.
        /// </summary>
        /// <param name="record">Запись документа для добавления.</param>
        /// <param name="assemblyIds">Список идентификаторов сборок, связанных с записью.</param>
        /// <exception cref="ArgumentNullException">Вызывается, если record равен null.</exception>
        Task AddRecordAsync(DocumentDetailRecord record, List<int> assemblyIds);

        /// <summary>
        /// Обновляет существующую запись документа и её связи с сборками.
        /// </summary>
        /// <param name="record">Запись документа для обновления.</param>
        /// <param name="assemblyIds">Список идентификаторов сборок, связанных с записью.</param>
        /// <exception cref="ArgumentNullException">Вызывается, если record равен null.</exception>
        Task UpdateRecordAsync(DocumentDetailRecord record, List<int> assemblyIds);

        /// <summary>
        /// Удаляет запись документа по её идентификатору.
        /// </summary>
        /// <param name="recordId">Идентификатор записи документа.</param>
        Task DeleteRecordAsync(int recordId);

        /// <summary>
        /// Получает список всех сборок с их ESKD-номерами и классификаторами.
        /// </summary>
        /// <returns>Список сборок.</returns>
        Task<List<Assembly>> GetAssembliesAsync();

        /// <summary>
        /// Получает список всех продуктов с их ESKD-номерами и классификаторами.
        /// </summary>
        /// <returns>Список продуктов.</returns>
        Task<List<Product>> GetProductsAsync();

        /// <summary>
        /// Удаляет сборку по её идентификатору.
        /// </summary>
        /// <param name="assemblyId">Идентификатор сборки.</param>
        Task DeleteAssemblyAsync(int assemblyId);

        /// <summary>
        /// Удаляет продукт по его идентификатору.
        /// </summary>
        /// <param name="productId">Идентификатор продукта.</param>
        Task DeleteProductAsync(int productId);

        /// <summary>
        /// Добавляет новую сборку в базу данных.
        /// </summary>
        /// <param name="assembly">Сборка для добавления.</param>
        /// <exception cref="ArgumentNullException">Вызывается, если assembly равен null.</exception>
        Task AddAssemblyAsync(Assembly assembly);

        /// <summary>
        /// Обновляет существующую сборку в базе данных.
        /// </summary>
        /// <param name="assembly">Сборка для обновления.</param>
        /// <exception cref="ArgumentNullException">Вызывается, если assembly равен null.</exception>
        Task UpdateAssemblyAsync(Assembly assembly);

        /// <summary>
        /// Добавляет новый продукт в базу данных.
        /// </summary>
        /// <param name="product">Продукт для добавления.</param>
        /// <exception cref="ArgumentNullException">Вызывается, если product равен null.</exception>
        Task AddProductAsync(Product product);

        /// <summary>
        /// Обновляет существующий продукт в базе данных.
        /// </summary>
        /// <param name="product">Продукт для обновления.</param>
        /// <exception cref="ArgumentNullException">Вызывается, если product равен null.</exception>
        Task UpdateProductAsync(Product product);

        /// <summary>
        /// Получает или создаёт классификатор по указанному коду.
        /// </summary>
        /// <param name="code">Код классификатора.</param>
        /// <returns>Объект классификатора или null, если код недействителен.</returns>
        Task<Classifier> GetOrCreateClassifierAsync(string code);

        /// <summary>
        /// Получает список продуктов, связанных с указанной сборкой.
        /// </summary>
        /// <param name="assemblyId">Идентификатор сборки.</param>
        /// <returns>Список продуктов, связанных с указанной сборкой.</returns>
        Task<List<Product>> GetProductsByAssemblyId(int assemblyId);

        /// <summary>
        /// Получает список родительских сборок для указанной сборки.
        /// </summary>
        /// <param name="assemblyId">Идентификатор дочерней сборки.</param>
        /// <returns>Список родительских сборок.</returns>
        Task<List<Assembly>> GetParentAssembliesAsync(int assemblyId);

        /// <summary>
        /// Обновляет связи между указанной сборкой и её родительскими сборками.
        /// </summary>
        /// <param name="assemblyId">Идентификатор дочерней сборки.</param>
        /// <param name="parentAssemblies">Список родительских сборок.</param>
        Task UpdateAssemblyParentAssembliesAsync(int assemblyId, List<Assembly> parentAssemblies);

        /// <summary>
        /// Обновляет связи между указанной сборкой и связанными продуктами.
        /// </summary>
        /// <param name="assemblyId">Идентификатор сборки.</param>
        /// <param name="relatedProducts">Список связанных продуктов.</param>
        Task UpdateAssemblyRelatedProductsAsync(int assemblyId, List<Product> relatedProducts);

        /// <summary>
        /// Обновляет связи между указанным продуктом и его родительскими сборками.
        /// </summary>
        /// <param name="productId">Идентификатор продукта.</param>
        /// <param name="parentAssemblies">Список родительских сборок.</param>
        Task UpdateProductParentAssembliesAsync(int productId, List<Assembly> parentAssemblies);

        /// <summary>
        /// Получает список родительских сборок для указанного продукта.
        /// </summary>
        /// <param name="productId">Идентификатор продукта.</param>
        /// <returns>Список родительских сборок.</returns>
        Task<List<Assembly>> GetProductParentAssembliesAsync(int productId);

        /// <summary>
        /// Получает список продуктов, связанных с указанной сборкой.
        /// </summary>
        /// <param name="assemblyId">Идентификатор сборки.</param>
        /// <returns>Список связанных продуктов.</returns>
        Task<List<Product>> GetRelatedProductsAsync(int assemblyId);

        /// <summary>
        /// Получает список родительских сборок для указанной детали.
        /// </summary>
        /// <param name="detailId">Идентификатор детали.</param>
        /// <returns>Список родительских сборок.</returns>
        Task<List<Assembly>> GetParentAssembliesForDetailAsync(int detailId);

        Task CreateProductWithAssembliesAsync(Product product, List<int> ParentassemblyIds);
        Task<Assembly> ConvertProductToAssemblyAsync(int productId, List<Product> childProducts);
        Task<List<ChangeLog>> GetChangesSince(DateTime timestamp);
    }
}
