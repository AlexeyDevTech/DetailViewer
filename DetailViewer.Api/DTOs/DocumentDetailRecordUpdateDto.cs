using DetailViewer.Api.Models;

namespace DetailViewer.Api.DTOs
{
    public class DocumentDetailRecordUpdateDto
    {
        public DocumentDetailRecord Record { get; set; }
        public List<int> AssemblyIds { get; set; }
        /// <summary>
        /// Получает или устанавливает список идентификаторов изделий, связанных с записью о детали документа.
        /// </summary>
        public List<int>? ProductIds { get; set; }
    }
}
