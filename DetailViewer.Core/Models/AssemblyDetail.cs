
namespace DetailViewer.Core.Models
{
    /// <summary>
    /// Промежуточная сущность для связи "многие-ко-многим" между Assembly и DocumentDetailRecord.
    /// </summary>
    public class AssemblyDetail
    {
        public int AssemblyId { get; set; }
        public virtual Assembly Assembly { get; set; }

        public int DetailId { get; set; }
        public virtual DocumentDetailRecord Detail { get; set; }
    }
}
