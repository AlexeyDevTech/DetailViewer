
#pragma warning disable CS8618

namespace DetailViewer.Api.Models
{
    /// <summary>
    /// Промежуточная сущность для связи "многие-ко-многим" между Product и Assembly.
    /// </summary>
    public class ProductAssembly
    {
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public int AssemblyId { get; set; }
        public virtual Assembly Assembly { get; set; }
    }
}
