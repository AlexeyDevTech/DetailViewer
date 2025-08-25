using DetailViewer.Api.Models;
using System.Collections.Generic;

namespace DetailViewer.Api.DTOs
{
    public class DocumentDetailRecordCreateDto
    {
        public DocumentDetailRecord Record { get; set; }
        public ESKDNumber EskdNumber { get; set; }
        public List<int> AssemblyIds { get; set; }
    }
}