#pragma warning disable CS8618

using System.Collections.Generic;

namespace DetailViewer.Api.Models.Dtos
{
    public class DocumentDetailRecordDto : DocumentDetailRecord
    {
        public List<int> ParentAssemblyIds { get; set; }
    }
}
