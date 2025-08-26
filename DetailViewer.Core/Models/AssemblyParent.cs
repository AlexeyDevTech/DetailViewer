using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DetailViewer.Core.Models
{
    public class AssemblyParent
    {
        [Key]
        public int Id { get; set; }

        public int ParentAssemblyId { get; set; }

        [ForeignKey("ParentAssemblyId")]
        public Assembly ParentAssembly { get; set; }

        public int ChildAssemblyId { get; set; }

        [ForeignKey("ChildAssemblyId")]
        public Assembly ChildAssembly { get; set; }
    }
}
