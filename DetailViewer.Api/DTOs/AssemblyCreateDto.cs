using DetailViewer.Api.Models;
using System.Collections.Generic;

namespace DetailViewer.Api.DTOs
{
    public class AssemblyCreateDto
    {
        /// <summary>
        /// Получает или устанавливает объект сборки.
        /// </summary>
        public Assembly Assembly { get; set; }
        /// <summary>
        /// Получает или устанавливает объект номера ЕСКД, связанный со сборкой.
        /// </summary>
        public ESKDNumber EskdNumber { get; set; }
        /// <summary>
        /// Получает или устанавливает список идентификаторов родительских сборок.
        /// </summary>
        public List<int> ParentAssemblyIds { get; set; }
        /// <summary>
        /// Получает или устанавливает список идентификаторов связанных продуктов.
        /// </summary>
        public List<int> RelatedProductIds { get; set; }
    }
}
