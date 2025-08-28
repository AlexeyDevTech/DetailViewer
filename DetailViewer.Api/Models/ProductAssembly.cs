
#pragma warning disable CS8618

namespace DetailViewer.Api.Models
{
    /// <summary>
    /// Промежуточная сущность для связи "многие-ко-многим" между Product и Assembly.
    /// </summary>
    public class ProductAssembly
    {
        /// <summary>
        /// Получает или устанавливает идентификатор продукта.
        /// </summary>
        public int ProductId { get; set; }
        /// <summary>
        /// Получает или устанавливает навигационное свойство для продукта.
        /// </summary>
        public virtual Product Product { get; set; }

        /// <summary>
        /// Получает или устанавливает идентификатор сборки.
        /// </summary>
        public int AssemblyId { get; set; }
        /// <summary>
        /// Получает или устанавливает навигационное свойство для сборки.
        /// </summary>
        public virtual Assembly Assembly { get; set; }
    }
}
