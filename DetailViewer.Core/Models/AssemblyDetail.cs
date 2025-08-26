namespace DetailViewer.Core.Models
{
    /// <summary>
    /// Промежуточная сущность для связи "многие-ко-многим" между Assembly и DocumentDetailRecord.
    /// </summary>
    public class AssemblyDetail
    {
        /// <summary>
        /// Внешний ключ для сборки.
        /// </summary>
        public int AssemblyId { get; set; }

        /// <summary>
        /// Навигационное свойство к сборке.
        /// </summary>
        public virtual Assembly Assembly { get; set; }

        /// <summary>
        /// Внешний ключ для детали (записи документа).
        /// </summary>
        public int DetailId { get; set; }

        /// <summary>
        /// Навигационное свойство к детали.
        /// </summary>
        public virtual DocumentDetailRecord Detail { get; set; }
    }
}