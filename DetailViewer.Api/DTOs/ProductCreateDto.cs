using DetailViewer.Api.Models;
using System.Collections.Generic;

namespace DetailViewer.Api.DTOs
{
    public class ProductCreateDto
    {
        /// <summary>
        /// Получает или устанавливает объект продукта.
        /// </summary>
        public Product Product { get; set; }
        /// <summary>
        /// Получает или устанавливает объект номера ЕСКД, связанный с продуктом.
        /// </summary>
        public ESKDNumber EskdNumber { get; set; }
        /// <summary>
        /// Получает или устанавливает список идентификаторов родительских сборок.
        /// </summary>
        public List<int> ParentAssemblyIds { get; set; }
    }
}
