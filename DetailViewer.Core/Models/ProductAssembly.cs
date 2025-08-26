namespace DetailViewer.Core.Models
{
    /// <summary>
    /// Промежуточная сущность для связи "многие-ко-многим" между Product и Assembly.
    /// </summary>
    public class ProductAssembly
    {
        /// <summary>
        /// Внешний ключ для продукта.
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Навигационное свойство к продукту.
        /// </summary>
        public virtual Product Product { get; set; }

        /// <summary>
        /// Внешний ключ для сборки.
        /// </summary>
        public int AssemblyId { get; set; }

        /// <summary>
        /// Навигационное свойство к сборке.
        /// </summary>
        public virtual Assembly Assembly { get; set; }
    }
}