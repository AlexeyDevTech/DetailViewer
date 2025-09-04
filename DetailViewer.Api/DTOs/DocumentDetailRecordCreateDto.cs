using DetailViewer.Api.Models;
using System.Collections.Generic;

namespace DetailViewer.Api.DTOs
{
    public class DocumentDetailRecordCreateDto
    {
        /// <summary>
        /// Получает или устанавливает объект записи о детали документа.
        /// </summary>
        public DocumentDetailRecord Record { get; set; }
        /// <summary>
        /// Получает или устанавливает объект номера ЕСКД, связанный с записью о детали документа.
        /// </summary>
        public ESKDNumber EskdNumber { get; set; }
        /// <summary>
        /// Получает или устанавливает список идентификаторов сборок, связанных с записью о детали документа.
        /// </summary>
        public List<int>? AssemblyIds { get; set; }
        /// <summary>
        /// Получает или устанавливает список идентификаторов изделий, связанных с записью о детали документа.
        /// </summary>
        public List<int>? ProductIds { get; set; }
    }
}
