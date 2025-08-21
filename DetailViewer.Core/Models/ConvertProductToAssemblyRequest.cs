using System.Collections.Generic;

namespace DetailViewer.Core.Models
{
    public class ConvertProductToAssemblyRequest
    {
        public int ProductId { get; set; }
        public List<int> ChildProductIds { get; set; }
    }
}
