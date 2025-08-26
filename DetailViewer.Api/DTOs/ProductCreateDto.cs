using DetailViewer.Api.Models;
using System.Collections.Generic;

namespace DetailViewer.Api.DTOs
{
    public class ProductCreateDto
    {
        public Product Product { get; set; }
        public ESKDNumber EskdNumber { get; set; }
        public List<int> ParentAssemblyIds { get; set; }
    }
}
