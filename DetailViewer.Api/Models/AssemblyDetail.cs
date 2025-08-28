
#pragma warning disable CS8618

namespace DetailViewer.Api.Models
{
    /// <summary>
    /// Промежуточная сущность для связи "многие-ко-многим" между Assembly и DocumentDetailRecord.
    /// </summary>
    public class AssemblyDetail
    {
        /// <summary>
        /// Получает или устанавливает идентификатор сборки.
        /// </summary>
        public int AssemblyId { get; set; }
        /// <summary>
        /// Получает или устанавливает навигационное свойство для сборки.
        /// </summary>
        public virtual Assembly Assembly { get; set; }

        /// <summary>
        /// Получает или устанавливает идентификатор детали документа.
        /// </summary>
        public int DetailId { get; set; }
        /// <summary>
        /// Получает или устанавливает навигационное свойство для детали документа.
        /// </summary>
        public virtual DocumentDetailRecord Detail { get; set; }
    }
}
