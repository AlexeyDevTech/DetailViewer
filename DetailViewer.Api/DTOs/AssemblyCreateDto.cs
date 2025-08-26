using DetailViewer.Api.Models;
using System.Collections.Generic;

namespace DetailViewer.Api.DTOs
{
    public class AssemblyCreateDto
    {
        public Assembly Assembly { get; set; }
        public ESKDNumber EskdNumber { get; set; }
        public List<int> ParentAssemblyIds { get; set; }
        public List<int> RelatedProductIds { get; set; }
    }
}
