using DetailViewer.Api.Models;

namespace DetailViewer.Api.DTOs
{
    public class DocumentDetailRecordUpdateDto
    {
        public DocumentDetailRecord Record { get; set; }
        public List<int> AssemblyIds { get; set; }
    }
}
